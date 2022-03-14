using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.DataTransferObjects;
using HLServiceRole.EntityFramework;
using System.Diagnostics;
using System.Collections;
using Microsoft.WindowsAzure.StorageClient;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        /// <summary>
        /// This ArrayList is used to temporary store the MediaIDs of those photos which the PhotoResizingWorkerRole has finished processing.
        /// </summary>
        static ArrayList finishedPhotos = new ArrayList();

        /// <summary>
        /// This ArrayList is used to temporary store the MediaIDs of those videos which the VideoConversionWorkerRole has finished processing.
        /// </summary>
        static ArrayList finishedVideos = new ArrayList();

        /// <summary>
        /// Gets the photos pertaining to a news item.
        /// </summary>
        /// <param name="newsItemId"></param>
        /// <returns></returns>
        public List<NewsItemPhotoDto> GetNewsItemPhotosBL(int newsItemId)
        {
            try
            {
                var photos = entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItemId);

                var newsItemPhotoDtos = new List<NewsItemPhotoDto>();

                foreach (var p in photos)
                {
                    newsItemPhotoDtos.Add
                    (
                        new NewsItemPhotoDto()
                        {
                            MediaID = p.MediaID,
                            NewsItemID = p.NewsItemID,
                            Caption = p.Caption,
                            BlobUri = GetSasUriForBlobReadBL(p.BlobUri), /* We must get a SAS in order to read the blobs. */
                            MediumSizeBlobUri = GetSasUriForBlobReadBL(p.MediumSizeBlobUri),
                            ThumbnailBlobUri = GetSasUriForBlobReadBL(p.ThumbnailBlobUri)
                        }
                    );
                }
                return newsItemPhotoDtos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.GetNewsItemPhotosBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the videos pertaining to a news item.
        /// </summary>
        /// <param name="newsItemId"></param>
        /// <returns></returns>
        public List<NewsItemVideoDto> GetNewsItemVideosBL(int newsItemId)
        {
            try
            {
                var videos = entityFramework.Media.OfType<NewsItemVideo>().Where(v => v.NewsItemID == newsItemId);

                var newsItemVideoDtos = new List<NewsItemVideoDto>();

                foreach (var v in videos)
                {
                    newsItemVideoDtos.Add
                    (
                        new NewsItemVideoDto()
                        {
                            MediaID = v.MediaID,
                            NewsItemID = v.NewsItemID,
                            Title = v.Title,
                            BlobUri = GetSasUriForBlobReadBL(v.BlobUri) /* We must get a SAS in order to read the blob. */
                        }
                    );
                }
                return newsItemVideoDtos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.GetNewsItemVideosBL(): " + ex.ToString());
                return null;
            }
        }


        private bool DoesNewsItemHavePhoto(int newsItemId)
        {
            try
            {
                var photos = entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItemId);

                if (photos != null)
                    if (photos.Count() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.DoesNewsItemHavePhoto(): " + ex.ToString());
                return false;
            }
        }


        private bool DoesNewsItemHaveVideo(int newsItemId)
        {
            try
            {
                var videos = entityFramework.Media.OfType<NewsItemVideo>().Where(p => p.NewsItemID == newsItemId);

                if (videos != null)
                    if (videos.Count() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.DoesNewsItemHaveVideo(): " + ex.ToString());
                return false;
            }
        }


        private bool DoesCommunityHavePhoto(int communityId)
        {
            try
            {
                var photos = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == communityId);

                if (photos != null)
                    if (photos.Count() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.DoesCommunityHavePhoto(): " + ex.ToString());
                return false;
            }
        }


        private bool DoesUserHavePhoto(int userId)
        {
            try
            {
                var photos = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == userId);

                if (photos != null)
                    if (photos.Count() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.DoesUserHavePhoto(): " + ex.ToString());
                return false;
            }
        }


        private bool DoesAssignmentHavePhoto(int assignmentId)
        {
            try
            {
                var photos = entityFramework.Media.OfType<AssignmentPhoto>().Where(p => p.AssignmentID == assignmentId);

                if (photos != null)
                    if (photos.Count() > 0)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.DoesAssignmentHavePhoto(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// A generic method which is the entry point to saving all media in the system. Be it photo or video. And belonging
        /// to different objects: NewsItem, Community, User, Assignment.
        /// </summary>
        /// <param name="hostItemId">
        /// This is NewsItemID, UserID, CommunityID or AssignmentID, it's not unique in itself, but in conjunction
        /// with the enum MediaUsage (News, Community, User, Assignment) there is uniqueness.
        /// </param>
        /// <param name="blobUri"></param>
        /// <param name="mediaUsage"></param>
        /// <returns></returns>
        public bool SaveMediaBL(int hostItemId, string blobUri, MediaUsage mediaUsage)
        {
            switch (mediaUsage)
            {
                case MediaUsage.News:
                    var mediaType = GetMediaType(blobUri);
                    switch (mediaType)
                    {
                        case MediaType.Photo:
                            SaveNewsItemPhoto(hostItemId, blobUri);
                            break;
                        case MediaType.Video:
                            SaveNewsItemVideo(hostItemId, blobUri);
                            break;
                    }
                    break;

                case MediaUsage.User:
                    SaveSinglePhoto(hostItemId, blobUri, MediaUsage.User);
                    break;

                case MediaUsage.Community:
                    SaveSinglePhoto(hostItemId, blobUri, MediaUsage.Community);
                    break;

                case MediaUsage.Assignment:
                    SaveSinglePhoto(hostItemId, blobUri, MediaUsage.Assignment);
                    break;
            }
            return true;
        }


        public bool SaveMediaFromPhoneBL(int newsItemId, byte[] photo)
        {
            try
            {
                var sasUri = GetSasUriForBlobWriteBL(MediaUsage.News, "fileFromPhone.jpg");
                var writeBlob = new CloudBlob(sasUri);

                // Set the content-type value for the blob.
                writeBlob.Properties.ContentType = "jpg";
                writeBlob.UploadByteArray(photo);

                var uri = GetUriWithoutSas(sasUri);
                SaveMediaBL(newsItemId, uri, MediaUsage.News);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MediaBL.SaveMediaFromPhoneBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// The objects Community, User, Assignment only stores a single photo with them. In contrast to a NewsItem,
        /// which can store multiple photos or videos with it. This method is used by the objects Community, User, 
        /// Assignment to store that single photo. In case there already exists a photo attached to the object, it 
        /// will be deleted and replaced by the new photo.
        /// </summary>
        /// <param name="hostItemId">
        /// This is NewsItemID, UserID, CommunityID or AssignmentID, it's not unique in itself, but in conjunction
        /// with the enum MediaUsage (News, Community, User, Assignment) there is uniqueness.
        /// </param>
        /// <param name="blobUri"></param>
        /// <param name="mediaUsage"></param>
        /// <returns></returns>
        private bool SaveSinglePhoto(int hostItemId, string blobUri, MediaUsage mediaUsage)
        {
            switch (mediaUsage)
            {
                case MediaUsage.User:
                    try
                    {
                        var photo = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == hostItemId).SingleOrDefault();

                        /* Deleting an existing photo, if it exists. Before saving the new photo. */
                        if (photo != null)
                        {
                            var mediaId = photo.MediaID;
                            DeleteMediaBL(mediaId, MediaUsage.User);
                        }
                        /* Saving the new photo. */
                        SaveUserPhoto(hostItemId, blobUri);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Problem in BusinessLogic.SaveSinglePhoto(): " + ex.ToString());
                        return false;
                    }
                    break;

                case MediaUsage.Community:
                    try
                    {
                        var photo = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == hostItemId).SingleOrDefault();

                        /* Deleting an existing photo, if it exists. Before saving the new photo. */
                        if (photo != null)
                        {
                            var mediaId = photo.MediaID;
                            DeleteMediaBL(mediaId, MediaUsage.Community);
                        }
                        /* Saving the new photo. */
                        SaveCommunityPhoto(hostItemId, blobUri);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Problem in BusinessLogic.SaveSinglePhoto(): " + ex.ToString());
                        return false;
                    }
                    break;

                case MediaUsage.Assignment:
                    try
                    {
                        var photo = entityFramework.Media.OfType<AssignmentPhoto>().Where(p => p.AssignmentID == hostItemId).SingleOrDefault();

                        /* Deleting an existing photo, if it exists. Before saving the new photo. */
                        if (photo != null)
                        {
                            var mediaId = photo.MediaID;
                            DeleteMediaBL(mediaId, MediaUsage.Assignment);
                        }
                        /* Saving the new photo. */
                        SaveAssignmentPhoto(hostItemId, blobUri);
                    }
                    catch (Exception ex)
                    {
                        Trace.TraceError("Problem in BusinessLogic.SaveSinglePhoto(): " + ex.ToString());
                        return false;
                    }
                    break;
            }
            return true;
        }


        public int SaveNewsItemPhoto(int newsItemId, string blobUri)
        {
            try
            {
                Trace.TraceInformation("Entered the SaveNewsItemPhoto method.");

                var newsItemPhoto = new EntityFramework.NewsItemPhoto();

                var isolatedFileName = GetIsolatedFileName(blobUri);

                newsItemPhoto.NewsItemID = newsItemId;
                newsItemPhoto.Caption = isolatedFileName;
                newsItemPhoto.BlobUri = blobUri;
                newsItemPhoto.MediumSizeBlobUri = string.Empty;
                newsItemPhoto.ThumbnailBlobUri = string.Empty;
                newsItemPhoto.CreateUpdateDate = DateTime.Now;

                entityFramework.Media.AddObject(newsItemPhoto);
                entityFramework.SaveChanges();

                var newMediaId = newsItemPhoto.MediaID;

                /* Puts a photo resizing message in the photoresizing photoResizingQueue to be processed by the PhotoResizingWorkerRole. */
                QueuePhotoResizingMessage(newMediaId, blobUri);

                /* Getting a reference to the videoConversionStatusQueue. */
                var photoResizingStatusQueue = queueStorage.GetQueueReference("photoresizingstatus");

                /* Flag which is used to signal when the PhotoResizingWorkerRole has finished resizing a photo. 
                 * When we photoResizingQueue a new photo to be processed by the PhotoResizingWorkerRole we put the videoConversionFinished flag to false, in order for
                 * the PhotoResizingWorkerRole to signal back by adding a message, which contains the resized photos MediaID, to the videoConversionStatusQueue
                 * All this is because we need for the media processing to be finished before we can redirect to the details view (viewing the processed
                 * media) in the clients. */
                bool photoResizingFinished = false;
                while (!photoResizingFinished)
                {
                    var mediaIdOfResizedPhotoMessage = photoResizingStatusQueue.GetMessage();

                    /* Adds the MediaID of the resized photo to the finishedPhotos ArrayList. */
                    if (mediaIdOfResizedPhotoMessage != null)
                    {
                        var mediaIdOfResizedPhoto = ConvertCloudQueueMessageToInt(mediaIdOfResizedPhotoMessage);
                        finishedPhotos.Add(mediaIdOfResizedPhoto);
                    }

                    /* When the finishedPhotos contains the MediaID of the photo we send off to be resized we know that the photo resizing is complete. */
                    if (finishedPhotos.Contains(newMediaId))
                    {
                        /* Remove the MediaID from the finishedPhotos ArrayList. */
                        finishedPhotos.Remove(newMediaId);
                        /* Remove message from videoConversionStatusQueue. */

                        photoResizingStatusQueue.DeleteMessage(mediaIdOfResizedPhotoMessage);
                        /* Setting the videoConversionFinished to true which means that we stop looping around here. */
                        photoResizingFinished = true;
                    }
                    System.Threading.Thread.Sleep(100);
                    Trace.TraceInformation("SaveNewsItemPhoto(): PhotoResizingWorkerRole has not finished yet.");
                }
                return newMediaId;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.SaveNewsItemPhoto(): " + ex.ToString());
                return -1;
            }
        }


        public int SaveUserPhoto(int userId, string blobUri)
        {
            try
            {
                Trace.TraceInformation("Entered the SaveUserPhoto method.");

                var userPhoto = new EntityFramework.UserPhoto();

                var isolatedFileName = GetIsolatedFileName(blobUri);

                userPhoto.UserID = userId;
                userPhoto.BlobUri = blobUri;
                userPhoto.MediumSizeBlobUri = string.Empty;
                userPhoto.ThumbnailBlobUri = string.Empty;
                userPhoto.CreateUpdateDate = DateTime.Now;

                entityFramework.Media.AddObject(userPhoto);

                entityFramework.SaveChanges();

                var newMediaId = userPhoto.MediaID;

                /* Puts a photo resizing message in the photoresizing photoResizingQueue to be processed by the PhotoResizingWorkerRole. */
                QueuePhotoResizingMessage(newMediaId, blobUri);

                /* Getting a reference to the videoConversionStatusQueue. */
                var photoResizingStatusQueue = queueStorage.GetQueueReference("photoresizingstatus");

                /* Flag which is used to signal when the PhotoResizingWorkerRole has finished resizing a photo. 
                 * When we photoResizingQueue a new photo to be processed by the PhotoResizingWorkerRole we put the videoConversionFinished flag to false, in order for
                 * the PhotoResizingWorkerRole to signal back by adding a message, which contains the resized photos MediaID, to the videoConversionStatusQueue
                 * All this is because we need for the media processing to be finished before we can redirect to the details view (viewing the processed
                 * media) in the clients. */
                bool photoResizingFinished = false;
                while (!photoResizingFinished)
                {
                    var mediaIdOfResizedPhotoMessage = photoResizingStatusQueue.GetMessage();

                    /* Adds the MediaID of the resized photo to the finishedPhotos ArrayList. */
                    if (mediaIdOfResizedPhotoMessage != null)
                    {
                        var mediaIdOfResizedPhoto = ConvertCloudQueueMessageToInt(mediaIdOfResizedPhotoMessage);
                        finishedPhotos.Add(mediaIdOfResizedPhoto);
                    }

                    /* When the finishedPhotos contains the MediaID of the photo we send off to be resized we know that the photo resizing is complete. */
                    if (finishedPhotos.Contains(newMediaId))
                    {
                        /* Remove the MediaID from the finishedPhotos ArrayList. */
                        finishedPhotos.Remove(newMediaId);
                        /* Remove message from videoConversionStatusQueue. */

                        photoResizingStatusQueue.DeleteMessage(mediaIdOfResizedPhotoMessage);
                        /* Setting the videoConversionFinished to true which means that we stop looping around here. */
                        photoResizingFinished = true;
                    }
                    System.Threading.Thread.Sleep(100);
                    Trace.TraceInformation("SaveUserPhoto(): PhotoResizingWorkerRole has not finished yet.");
                }
                return newMediaId;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.SaveUserPhoto(): " + ex.ToString());
                return -1;
            }
        }


        public int SaveCommunityPhoto(int communityId, string blobUri)
        {
            try
            {
                Trace.TraceInformation("Entered the SaveCommunityPhoto() method.");

                var photo = new EntityFramework.CommunityPhoto();

                var isolatedFileName = GetIsolatedFileName(blobUri);

                photo.CommunityID = communityId;
                photo.BlobUri = blobUri;
                photo.MediumSizeBlobUri = string.Empty;
                photo.ThumbnailBlobUri = string.Empty;
                photo.CreateUpdateDate = DateTime.Now;

                entityFramework.Media.AddObject(photo);

                entityFramework.SaveChanges();

                var newMediaId = photo.MediaID;

                /* Puts a photo resizing message in the photoresizing photoResizingQueue to be processed by the PhotoResizingWorkerRole. */
                QueuePhotoResizingMessage(newMediaId, blobUri);

                /* Getting a reference to the videoConversionStatusQueue. */
                var photoResizingStatusQueue = queueStorage.GetQueueReference("photoresizingstatus");

                /* Flag which is used to signal when the PhotoResizingWorkerRole has finished resizing a photo. 
                 * When we photoResizingQueue a new photo to be processed by the PhotoResizingWorkerRole we put the videoConversionFinished flag to false, in order for
                 * the PhotoResizingWorkerRole to signal back by adding a message, which contains the resized photos MediaID, to the videoConversionStatusQueue
                 * All this is because we need for the media processing to be finished before we can redirect to the details view (viewing the processed
                 * media) in the clients. */
                bool photoResizingFinished = false;
                while (!photoResizingFinished)
                {
                    var mediaIdOfResizedPhotoMessage = photoResizingStatusQueue.GetMessage();

                    /* Adds the MediaID of the resized photo to the finishedPhotos ArrayList. */
                    if (mediaIdOfResizedPhotoMessage != null)
                    {
                        var mediaIdOfResizedPhoto = ConvertCloudQueueMessageToInt(mediaIdOfResizedPhotoMessage);
                        finishedPhotos.Add(mediaIdOfResizedPhoto);
                    }

                    /* When the finishedPhotos contains the MediaID of the photo we send off to be resized we know that the photo resizing is complete. */
                    if (finishedPhotos.Contains(newMediaId))
                    {
                        /* Remove the MediaID from the finishedPhotos ArrayList. */
                        finishedPhotos.Remove(newMediaId);
                        /* Remove message from videoConversionStatusQueue. */

                        photoResizingStatusQueue.DeleteMessage(mediaIdOfResizedPhotoMessage);
                        /* Setting the videoConversionFinished to true which means that we stop looping around here. */
                        photoResizingFinished = true;
                    }
                    System.Threading.Thread.Sleep(100);
                    Trace.TraceInformation("SaveCommunityPhoto(): PhotoResizingWorkerRole has not finished yet.");
                }
                return newMediaId;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.SaveCommunityPhoto(): " + ex.ToString());
                return -1;
            }
        }


        public int SaveAssignmentPhoto(int assignmentId, string blobUri)
        {
            try
            {
                Trace.TraceInformation("Entered the SaveAssignmentPhoto() method.");

                var photo = new EntityFramework.AssignmentPhoto();

                var isolatedFileName = GetIsolatedFileName(blobUri);

                photo.AssignmentID = assignmentId;
                photo.BlobUri = blobUri;
                photo.MediumSizeBlobUri = string.Empty;
                photo.ThumbnailBlobUri = string.Empty;
                photo.CreateUpdateDate = DateTime.Now;

                entityFramework.Media.AddObject(photo);

                entityFramework.SaveChanges();

                var newMediaId = photo.MediaID;

                /* Puts a photo resizing message in the photoresizing photoResizingQueue to be processed by the PhotoResizingWorkerRole. */
                QueuePhotoResizingMessage(newMediaId, blobUri);

                /* Getting a reference to the videoConversionStatusQueue. */
                var photoResizingStatusQueue = queueStorage.GetQueueReference("photoresizingstatus");

                /* Flag which is used to signal when the PhotoResizingWorkerRole has finished resizing a photo. 
                 * When we photoResizingQueue a new photo to be processed by the PhotoResizingWorkerRole we put the videoConversionFinished flag to false, in order for
                 * the PhotoResizingWorkerRole to signal back by adding a message, which contains the resized photos MediaID, to the videoConversionStatusQueue
                 * All this is because we need for the media processing to be finished before we can redirect to the details view (viewing the processed
                 * media) in the clients. */
                bool photoResizingFinished = false;
                while (!photoResizingFinished)
                {                  
                    var mediaIdOfResizedPhotoMessage = photoResizingStatusQueue.GetMessage();

                    /* Adds the MediaID of the resized photo to the finishedPhotos ArrayList. */
                    if (mediaIdOfResizedPhotoMessage != null)
                    {
                        var mediaIdOfResizedPhoto = ConvertCloudQueueMessageToInt(mediaIdOfResizedPhotoMessage);
                        finishedPhotos.Add(mediaIdOfResizedPhoto);
                    }

                    /* When the finishedPhotos contains the MediaID of the photo we send off to be resized we know that the photo resizing is complete. */
                    if (finishedPhotos.Contains(newMediaId))
                    {
                        /* Remove the MediaID from the finishedPhotos ArrayList. */
                        finishedPhotos.Remove(newMediaId);
                        /* Remove message from videoConversionStatusQueue. */

                        photoResizingStatusQueue.DeleteMessage(mediaIdOfResizedPhotoMessage);
                        /* Setting the videoConversionFinished to true which means that we stop looping around here. */
                        photoResizingFinished = true;
                    }
                    System.Threading.Thread.Sleep(100);
                    Trace.TraceInformation("SaveAssignmentPhoto(): PhotoResizingWorkerRole has not finished yet.");
                }
                return newMediaId;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.SaveAssignmentPhoto(): " + ex.ToString());
                return -1;
            }
        }


        public int SaveNewsItemVideo(int newsItemId, string blobUri)
        {
            try
            {
                var newsItemVideo = new EntityFramework.NewsItemVideo();

                var isolatedFileName = GetIsolatedFileName(blobUri);

                newsItemVideo.NewsItemID = newsItemId;
                newsItemVideo.Title = isolatedFileName;
                newsItemVideo.BlobUri = blobUri;
                newsItemVideo.CreateUpdateDate = DateTime.Now;

                entityFramework.Media.AddObject(newsItemVideo);
                entityFramework.SaveChanges();

                var newMediaId = newsItemVideo.MediaID;

                /* Puts a video conversion message in the videoconversion photoResizingQueue to be processed by the VideoConversionWorkerRole. */
                QueueVideoConversionMessage(newMediaId, blobUri);

                /* Getting a reference to the videoConversionStatusQueue. */
                var videoConversionStatusQueue = queueStorage.GetQueueReference("videoconversionstatus");

                /* When we queue a new video to be processed by the VideoConversionWorkerRole we put the videoConversionFinished flag to false, in order for
                 * the VideoConversionWorkerRole to signal back by adding a message, which contains the converted videos MediaID, to the videoConversionStatusQueue
                 * All this is because we need for the media processing to be finished before we can redirect to the details view (viewing the processed
                 * media) in the clients. */
                bool videoConversionFinished = false;
                while (!videoConversionFinished)
                {
                    var messageWithMediaIdOfConvertedVideo = videoConversionStatusQueue.GetMessage();

                    /* Adds the MediaID of the resized photo to the finishedPhotos ArrayList. */
                    if (messageWithMediaIdOfConvertedVideo != null)
                    {
                        var mediaIdOfConvertedVideo = ConvertCloudQueueMessageToInt(messageWithMediaIdOfConvertedVideo);
                        finishedVideos.Add(mediaIdOfConvertedVideo);
                    }

                    /* When the finishedPhotos contains the MediaID of the photo we send off to be resized we know that the photo resizing is complete. */
                    if (finishedVideos.Contains(newMediaId))
                    {
                        /* Remove the MediaID from the finishedPhotos ArrayList. */
                        finishedVideos.Remove(newMediaId);
                        /* Remove message from videoConversionStatusQueue. */

                        videoConversionStatusQueue.DeleteMessage(messageWithMediaIdOfConvertedVideo);
                        /* Setting the videoConversionFinished to true which means that we stop looping around here. */
                        videoConversionFinished = true;
                    }
                    System.Threading.Thread.Sleep(100);
                    Trace.TraceInformation("VideoConversionWorkerRole has not finished yet.");
                }
                return newMediaId;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.SaveNewsItemVideo(): " + ex.ToString());
                return -1;
            }
        }


        /// <summary>
        /// This is used by the PhotoResizingWorkerRole to update the database with the blob URIs to the resized photos.
        /// </summary>
        public bool UpdateResizedPhotoBlobUrisBL(int mediaId, string blobUriLargePhoto, string blobUriMediumPhoto, string blobUriThumbnailPhoto)
        {
            try
            {
                var photo = entityFramework.Media.OfType<Photo>().SingleOrDefault(m => m.MediaID == mediaId);

                if (photo != null)
                {
                    photo.BlobUri = blobUriLargePhoto;
                    photo.MediumSizeBlobUri = blobUriMediumPhoto;
                    photo.ThumbnailBlobUri = blobUriThumbnailPhoto;

                    entityFramework.SaveChanges();

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.UpdateResizedPhotoBlobUrisBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// This is used by the VideoConversionWorkerRole to update the database with the blob URI to the converted video.
        /// </summary>
        public bool UpdateConvertedVideoBlobUriBL(int mediaId, string blobUriConvertedVideo)
        {
            try
            {
                var video = entityFramework.Media.OfType<NewsItemVideo>().SingleOrDefault(m => m.MediaID == mediaId);

                if (video != null)
                {
                    video.BlobUri = blobUriConvertedVideo;
                    entityFramework.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.UpdateConvertedVideoBlobUriBL(): " + ex.ToString());
                return false;
            }
        }


        public bool UpdatePhotoCaptionBL(int mediaId, string photoCaptionText)
        {
            try
            {
                var photo = entityFramework.Media.OfType<NewsItemPhoto>().SingleOrDefault(m => m.MediaID == mediaId);

                if (photoCaptionText.Length == 0)
                    photoCaptionText = string.Empty;

                if (photo != null)
                {
                    photo.Caption = photoCaptionText;

                    entityFramework.SaveChanges();

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.UpdatePhotoCaptionBL(): " + ex.ToString());
                return false;
            }
        }


        public bool UpdateVideoTitleBL(int mediaId, string videoTitleText)
        {
            try
            {
                var video = entityFramework.Media.OfType<NewsItemVideo>().SingleOrDefault(m => m.MediaID == mediaId);

                if (videoTitleText.Length == 0)
                    videoTitleText = string.Empty;

                if (video != null)
                {
                    video.Title = videoTitleText;
                    entityFramework.SaveChanges();

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.UpdateVideoTitleBL(): " + ex.ToString());
                return false;
            }
        }


        public bool DeleteMediaBL(int mediaId, MediaUsage mediaUsage)
        {
            try
            {
                var media = entityFramework.Media.SingleOrDefault(m => m.MediaID == mediaId);

                if (media != null)
                {
                    var fileName = GetFileNameFromUri(media.BlobUri);
                    var mediaType = GetMediaType(fileName);

                    switch (mediaType)
                    {
                        case MediaType.Photo:
                            /* Deletes the blobs which stores the different sizes of the photo: Large, Medium and Thumbnail. */
                            DeletePhotoBlobs(mediaId);
                            switch (mediaUsage)
                            {
                                case MediaUsage.News:
                                    media = entityFramework.Media.OfType<NewsItemPhoto>().SingleOrDefault(m => m.MediaID == mediaId);
                                    break;
                                case MediaUsage.User:
                                    break;
                                case MediaUsage.Community:
                                    break;
                            }
                            break;
                        case MediaType.Video:
                            /* Deletes the blob which stores the video. */
                            DeleteVideoBlob(mediaId);
                            media = entityFramework.Media.OfType<NewsItemVideo>().SingleOrDefault(m => m.MediaID == mediaId);
                            break;
                    }
                    entityFramework.Media.DeleteObject(media);
                    entityFramework.SaveChanges();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.DeleteMediaBL(): " + ex.ToString());
                return false;
            }
        }
    }
}