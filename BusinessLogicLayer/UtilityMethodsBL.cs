using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;
using HLServiceRole.EntityFramework;
using System.Diagnostics;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        #region Assignment Utility Methods

        private string GetAssignmentTitle(int assignmentId)
        {
            try
            {
                var assignment = entityFramework.Assignments.Where(a => a.AssignmentID == assignmentId).SingleOrDefault();

                if (assignment != null)
                {
                    return assignment.Title;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.GetAssignmentTitle(): " + ex.ToString());
                return null;
            }
        }

        #endregion


        #region AzureBlob Utility Methods

        /// <summary>
        /// We are storing media for different usages (News, User, Community) in different blob containers.
        /// This method returns the name of the appropriate blob container for a particular usage. 
        /// </summary>
        /// <param name="fileName"></param>
        /// <param name="mediaUsage">News, User, Community</param>
        /// <returns>blob container names: newsitemphoto, userphoto, newsitemvideo, etc.</returns>
        private string GetBlobContainerNameForMedia(string fileName, MediaUsage mediaUsage)
        {
            var mediaType = GetMediaType(fileName);
            string containerName = string.Empty;

            switch (mediaType)
            {
                case MediaType.Photo:
                    switch (mediaUsage)
                    {
                        case MediaUsage.News:
                            containerName = "newsitemphoto";
                            break;
                        case MediaUsage.User:
                            containerName = "userphoto";
                            break;
                        case MediaUsage.Community:
                            containerName = "communityphoto";
                            break;
                        case MediaUsage.Assignment:
                            containerName = "assignmentphoto";
                            break;
                    }
                    break;
                case MediaType.Video:
                    containerName = "newsitemvideo";
                    break;
            }
            return containerName;
        }


        /// <summary>
        /// Takes in a file name and returns what media type (Photo or Video) it is.
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <returns>enum MediaType.Photo, MediaType.Video</returns>
        private MediaType GetMediaType(string fileName)
        {
            var fileExtension = GetFileExtension(fileName);

            MediaType mediaType = new MediaType();

            switch (fileExtension)
            {
                case "jpg":
                case "jpeg":
                case "gif":
                case "png":
                    mediaType = MediaType.Photo;
                    break;

                case "avi":
                case "flv":
                case "mov":
                case "mpg":
                case "mp4":
                case "m4v":
                case "wmv":
                case "3gp":
                    mediaType = MediaType.Video;
                    break;
            }
            return mediaType;
        }


        /// <summary>
        /// Takes a file name and returns it with a GUID appended. Thus uniqueness is guaranteed.
        /// </summary>
        private string GetUniqueFileName(string fileName)
        {
            var guid = Guid.NewGuid();
            var fileNameWithoutExt = GetFileNameWithoutExtension(fileName);
            var fileExtension = GetFileExtension(fileName);

            /* We store photo files in different formats, the "_O_" designates Original size. The resized photo files 
             * will later be stored with size indicators: _L_ Large, _M_ Medium and _T_ Thumbnail.
             * In the case of video files, the _O_ naturally indicates Original video file, where as the Converted
             * video file will be stored with a _C_ indicator. */
            var uniqueFileName = fileNameWithoutExt + "_O_" + guid + "." + fileExtension;

            return uniqueFileName;
        }


        /// <summary>
        /// Takes in a file name and returns the file extension.
        /// </summary>
        private string GetFileExtension(string fileName)
        {
            var fileNameParts = fileName.Split('.');
            var indexOfLastPart = fileNameParts.Length - 1;
            return fileNameParts[indexOfLastPart];
        }


        /// <summary>
        /// Takes in a file name and returns it without the file extension.
        /// </summary>
        private string GetFileNameWithoutExtension(string fileName)
        {
            var fileNameParts = fileName.Split('.');
            var indexOfLastPart = fileNameParts.Length - 1;
            var fileNameExtension = fileNameParts[indexOfLastPart];

            string[] fileNamePartsWithoutExtension = new string[indexOfLastPart];

            System.Array.Copy(fileNameParts, fileNamePartsWithoutExtension, indexOfLastPart);

            var fileNameWithoutExt = String.Join(".", fileNamePartsWithoutExtension);

            return fileNameWithoutExt;
        }


        /// <summary>
        /// Takes in the original blobUri and returns a new URI for resized photos.
        /// </summary>
        private string GetBlobUriForResizedPhoto(PhotoSize photoSize, string blobUri)
        {
            /* The blobUri we get in has a size indicator of "O" for Original  (The last "_O_" in the URI). Below we locate the index of this size indicator. */
            var photoSizeIndicator = blobUri.LastIndexOf("_O_");
            /* We remove the "O" size indicator. */
            var uriWithoutPhotoSizeIndicator = blobUri.Remove(photoSizeIndicator + 1, 1);
            var uriWithNewSizeIndicator = string.Empty;

            switch (photoSize)
            {
                case PhotoSize.Large:
                    /* We insert a new size indicator ("L", "M" or "T") in the URI. */
                    uriWithNewSizeIndicator = uriWithoutPhotoSizeIndicator.Insert(photoSizeIndicator + 1, "L");
                    break;
                case PhotoSize.Medium:
                    uriWithNewSizeIndicator = uriWithoutPhotoSizeIndicator.Insert(photoSizeIndicator + 1, "M");
                    break;
                case PhotoSize.Thumbnail:
                    uriWithNewSizeIndicator = uriWithoutPhotoSizeIndicator.Insert(photoSizeIndicator + 1, "T");
                    break;
            }
            return uriWithNewSizeIndicator;
        }


        /// <summary>
        /// Gets the blob URI for the converted video.
        /// </summary>
        private string GetBlobUriForConvertedVideo(string blobUri)
        {
            /* The blobUri we get in has a state indicator of "O" for Original  (The last "_O_" in the URI). Below we locate the index of this state indicator. */
            var videoStateIndicator = blobUri.LastIndexOf("_O_");
            /* We remove the "O" state indicator. */
            var uriWithoutVideoStateIndicator = blobUri.Remove(videoStateIndicator + 1, 1);
            /* We insert a state indicator _C_ to denote that the video has been Converted. */
            var uriWithNewStateIndicator = uriWithoutVideoStateIndicator.Insert(videoStateIndicator + 1, "C");
            /* Replace the current file extension with an .mp4 file extension. */
            var uriWithMp4FileExtension = ReplaceFileExtension(uriWithNewStateIndicator, "mp4");

            return uriWithMp4FileExtension;
        }


        /// <summary>
        /// Takes in an URI and returns a file name.
        /// </summary>
        private string GetFileNameFromUri(string uri)
        {
            var uriParts = uri.Split('/');
            var indexOfLastPart = uriParts.Length - 1;
            var fileName = uriParts[indexOfLastPart];

            return fileName;
        }


        /// <summary>
        /// Takes an URI with SAS and returns an URI without SAS.
        /// </summary>
        /// <param name="uri">URI with SAS.</param>
        /// <returns>URI without SAS.</returns>
        private string GetUriWithoutSas(string uri)
        {
            var uriParts = uri.Split('?');
            return uriParts[0];
        }


        /// <summary>
        /// Takes in an file URI and returns it with a new file extension.
        /// </summary>
        /// <param name="blobUri">file URI</param>
        /// <param name="newExtension">new file extension</param>
        private string ReplaceFileExtension(string blobUri, string newExtension)
        {
            /* Locate the dot before the file extension. */
            var lastDot = blobUri.LastIndexOf(".");

            /* Remove the current file extension. */
            var uriWithoutFileExtension = blobUri.Remove(lastDot + 1);

            /* Append the new file extension to the blob URI. */
            var uriWithNewFileExtension = uriWithoutFileExtension + newExtension;

            return uriWithNewFileExtension;
        }


        /// <summary>
        /// Returns an isolated file name, without extension, GUID and media state indicator.
        /// </summary>
        /// <param name="blobUri"></param>
        /// <returns>E.g.: "Lighthouse", "JellyFish", "Tulips".</returns>
        private string GetIsolatedFileName(string blobUri)
        {
            /* E.g. : "Lighthouse_O_7774a26f-b6ad-47be-b998-6338c483ac7a.jpg". */
            var fileName = GetFileNameFromUri(blobUri);
            /* E.g. : "Lighthouse_O_7774a26f-b6ad-47be-b998-6338c483ac7a". */
            var fileNameWithoutExtension = GetFileNameWithoutExtension(fileName);
            /* We locate the media state indicator "_O_", in order to remove it and the GUID. */
            var mediaStateIndicator = fileNameWithoutExtension.LastIndexOf("_O_");
            /* E.g. : "Lighthouse". - The file name where the state indicator and GUID is removed. */
            var isolatedFileName = fileNameWithoutExtension.Remove(mediaStateIndicator);

            return isolatedFileName;
        }


        /// <summary>
        /// Deletes the different blobs (Large, Medium, Thumbnail) containing a particular photo
        /// </summary>
        private bool DeletePhotoBlobs(int mediaId)
        {
            try
            {
                var photo = entityFramework.Media.OfType<Photo>().SingleOrDefault(m => m.MediaID == mediaId);

                if (photo != null)
                {
                    DeleteBlob(photo.BlobUri);
                    DeleteBlob(photo.MediumSizeBlobUri);
                    DeleteBlob(photo.ThumbnailBlobUri);

                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.DeletePhotoBlobs(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Deletes the blob which stores a video.
        /// </summary>
        private bool DeleteVideoBlob(int mediaId)
        {
            try
            {
                var video = entityFramework.Media.OfType<NewsItemVideo>().SingleOrDefault(m => m.MediaID == mediaId);

                if (video != null)
                {
                    DeleteBlob(video.BlobUri);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.DeleteVideoBlob(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Takes a CloudQueueMessage which contains an int and returns that int.
        /// </summary>
        private int ConvertCloudQueueMessageToInt(CloudQueueMessage message)
        {
            try
            {
                var messageAsString = message.AsString;
                var messageAsInt = Convert.ToInt32(messageAsString);
                return messageAsInt;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UtilityMethodsBL.ConvertCloudQueueMessageToInt(): " + ex.ToString());
                return -1;
            }
        }

        #endregion


        #region Comment Utility Methods

        /// <summary>
        /// The Comment PartionKey is made of the NewsItemID prefixed with a 0. This method strips off the 0 and return the NewsItemID.
        /// </summary>
        /// <param name="commentPartitionKey">Comment PartitionKey = "049"</param>
        /// <returns>NewsItemID = 49</returns>
        private int ConvertCommentPartionKeyToNewsItemId(string commentPartitionKey)
        {
            var trimmedCommentPartitionKey = commentPartitionKey.TrimStart('0');
            var newsItemIdInt = Convert.ToInt32(trimmedCommentPartitionKey);
            return newsItemIdInt;
        }


        /// <summary>
        /// The Comment PartionKey is made of the NewsItemID prefixed with a 0. This method adds that 0 and returns it as a string, e.g. "049".
        /// </summary>
        /// <param name="newsItemId">NewsItemID = 49</param>
        /// <returns>Comment PartitionKey = "049"</returns>
        private string ConvertNewsItemIdToCommentPartionKey(int newsItemId)
        {
            return "0" + newsItemId.ToString();
        }

        #endregion


        #region NewsItem Utility Methods

        private bool ConvertHasNextPageOfDataStringToBool(string hasNextPageOfData)
        {
            if (hasNextPageOfData == "yes")
                return true;
            else
                return false;
        }


        /* SEO on the page links was not implemented. */
        /// <summary>
        /// Generates a SEO friendly path for NewItems, Communities, Assignments and Users. We take the name or title from those objects and generate a SEO friendly path
        /// out of it. Illegal and disturbing characters will be removed and special Danish characters like æ, ø, å will be converted to ae, oe and aa.
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        //private string GenerateSeoPath(string path)
        //{
        //    // make the path lowercase
        //    string seoPath = (path ?? "").ToLower();

        //    // replace & with -
        //    seoPath = Regex.Replace(seoPath, @"\&+", "-");

        //    // remove characters
        //    seoPath = seoPath.Replace("'", "");

        //    // Replace("æ", "ae")
        //    seoPath = seoPath.Replace("æ", "ae");

        //    // Replace("å", "aa")
        //    seoPath = seoPath.Replace("å", "aa");

        //    // Replace("ø", "oe")
        //    seoPath = seoPath.Replace("ø", "oe");

        //    // remove invalid characters
        //    seoPath = Regex.Replace(seoPath, @"[^a-z0-9]", "-");

        //    // remove duplicates
        //    seoPath = Regex.Replace(seoPath, @"-+", "-");

        //    // trim leading & trailing characters
        //    seoPath = seoPath.Trim('-');

        //    return seoPath;
        //}

        #endregion


        #region PollBL Utility Methods

        /// <summary>
        /// Is used to convert a Poll AreaIdentifier (e.g. string: "c_4") to the CommunityID it represents (e.g. int: 4).
        /// </summary>
        /// <param name="areaIdentifier">Takes in a Poll AreaIdentifier string: "c_4".</param>
        /// <returns>Returns an int which is the CommunityID contained in the AreaIdentifier, e.g. 4.</returns>
        private int ExtractIdFromAreaIdentifier(string areaIdentifier)
        {
            // Removes the "c_" from the "c_4" AreaIdentifier.
            var id = areaIdentifier.Remove(0, 2);

            // Converts the "4" string to it's equivalent int 4.
            var idInt = Convert.ToInt32(id);

            return idInt;
        }


        /// <summary>
        /// Takes in a CommunityID (e.g. int 4) and returns an AreaIdentifier string (e.g. "c_4"), used by Poll objects.
        /// </summary>
        /// <param name="idInt">CommunityID, like 4.</param>
        /// <returns>Poll AreaIdentifier, like "c_4".</returns>
        private string ConvertIdIntToAreaIdentifier(int idInt)
        {
            // Converts the int to string.
            var id = idInt.ToString();

            // Preprend the ID with "c_".
            var areaIdentifier = "c_" + id;

            return areaIdentifier;
        }

        #endregion


        #region SpatialQuery Utility Methods

        /// <summary>
        /// Converts latitude/longitude coordinates to Point WKT
        /// Thus latitude: 57.0, longitude: 10.0 becomes "POINT (10.0 57.0)".
        /// </summary>
        private string ConvertLatLongToPointWkt(double longitude, double latitude)
        {
            return string.Format("POINT ({0} {1})", longitude, latitude);
        }


        /// <param name="pointWkt">"POINT (10.0 57.0)"</param>
        /// <returns>57.0</returns>
        private double ExtractLatitudeFromPointWkt(string pointWkt)
        {
            // The Point WKT is initially in this form: "POINT (-3.19 55.95)"
            // Removes "Point ("
            pointWkt = pointWkt.Remove(0, 7);
            // Removes ")"
            pointWkt = pointWkt.Trim(')');
            // Splits the remainder by the empty space.
            var parts = pointWkt.Split(' ');
            // Returns the latitude value.
            return Convert.ToDouble(parts[1]);
        }


        /// <param name="pointWkt">"POINT (10.0 57.0)"</param>
        /// <returns>10.0</returns>
        private double ExtractLongitudeFromPointWkt(string pointWkt)
        {
            // The Point WKT is initially in this form: "POINT (-3.19 55.95)"
            // Removes "Point ("
            pointWkt = pointWkt.Remove(0, 7);
            // Removes ")"
            pointWkt = pointWkt.Trim(')');
            // Splits the remainder by the empty space.
            var parts = pointWkt.Split(' ');
            // Returns the longitude value.
            return Convert.ToDouble(parts[0]);
        }

        #endregion


        #region User Utility Methods

        private string GetUserName(int userId)
        {
            var user = GetUserBL(userId);

            if (user != null)
            {
                return user.FirstName + " " + user.LastName;
            }
            else
            {
                return null;
            }
        }


        private string GetUserPhotoThumbnail(int userId)
        {
            var user = GetUserBL(userId);

            if (user != null)
            {
                return user.ThumbnailBlobUri;
            }
            else
            {
                return null;
            }
        }


        private string GetUserPhotoMedium(int userId)
        {
            var user = GetUserBL(userId);

            if (user != null)
            {
                return user.MediumSizeBlobUri;
            }
            else
            {
                return null;
            }
        }


        private int GetNumberOfNewsItemsPostedByUser(int userId)
        {
            try
            {
                var number = Convert.ToInt32(entityFramework.procGetNumberOfNewsItemsPostedByUser(userId).Single());
                return number;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.GetNumberOfNewsItemsPostedByUser(): " + ex.ToString());
                return 0;
            }
        }

        #endregion


        #region SendEmail Utility Methods

        /// <summary>
        /// Used for truncating text.
        /// </summary>
        private string Truncate(string input, int length)
        {
            if (input.Length <= length)
            {
                return input;
            }
            else
            {
                return input.Substring(0, length) + "...";
            }
        }

        #endregion
    }
}