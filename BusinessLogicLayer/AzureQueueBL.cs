using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;

namespace HLServiceRole.BusinessLogicLayer
{
    /// <summary>
    /// Overview of Azure Queue Storage
    /// The system uses 4 queues:
    /// - photoResizingQueue - Ships off photo to be resized - Flow: WCF role -> PhotoResizingWorkerRole.
    /// - photoResizingStatusQueue (PhotoResizingWorkerRole) - Signals that photo resizing is finished - Flow: PhotoResizingWorkerRole -> WCF role.
    /// - videoConversionQueue - Ships off video to be converted - Flow: WCF role -> VideoConversionWorkerRole.
    /// - videoConversionStatusQueue (VideoConversionWorkerRole) - Signals that video conversion is finished - Flow: VideoConversionWorkerRole -> WCF role.
    /// </summary>
    public partial class BusinessLogic
    {
        /// <summary>
        /// Puts a photo resizing message in the photoresizing photoResizingQueue, which is the way the
        /// HLServiceRole WCF web role communicates with the PhotoResizingWorkerRole.
        /// </summary>
        /// <param name="mediaId">MediaID of the photo.</param>
        /// <param name="blobUriOriginalPhoto">Blob URI of the original photo.</param>
        private void QueuePhotoResizingMessage(int mediaId, string blobUriOriginalPhoto)
        {
            /* Getting the blob URIs where we will store the resized photos. */
            var blobUriLargePhoto = GetBlobUriForResizedPhoto(PhotoSize.Large, blobUriOriginalPhoto);
            var blobUriMediumPhoto = GetBlobUriForResizedPhoto(PhotoSize.Medium, blobUriOriginalPhoto);
            var blobUriThumbnailPhoto = GetBlobUriForResizedPhoto(PhotoSize.Thumbnail, blobUriOriginalPhoto);

            /* Acquiring the appropriate Shared Access Signatures (SAS) allowing us to read, write and delete the blob URIs. */
            var sasReadBlobUriOriginalPhoto = GetSasUriForBlobReadBL(blobUriOriginalPhoto);
            var sasDeleteBlobUriOriginalPhoto = GetSasUriForBlobDeleteBL(blobUriOriginalPhoto);
            var sasWriteBlobUriLargePhoto = GetSasUriForBlobWrite(blobUriLargePhoto);
            var sasWriteBlobUriMediumPhoto = GetSasUriForBlobWrite(blobUriMediumPhoto);
            var sasWriteBlobUriThumbnailPhoto = GetSasUriForBlobWrite(blobUriThumbnailPhoto);

            /* Getting a reference to the photoresizing photoResizingQueue, where we will store the photo resizing message. */
            var queue = queueStorage.GetQueueReference("photoresizing");

            /* In the photoResizingQueue message we put the MediaID, basic URIs and SAS URIs, that is:
             * - MediaID of the photo. 
             * - basic URIs (Without SAS) of the Large, Medium and Thumbnail photos, which are to be stored in the database.
             * - SAS URIs, to Read/Delete O Original photo and Write L Large, M Medium, and T Thumbnail photos. */
            var message = new CloudQueueMessage(String.Format
            (
                "{0},{1},{2},{3},{4},{5},{6},{7},{8}",
                mediaId,
                blobUriLargePhoto, blobUriMediumPhoto, blobUriThumbnailPhoto,
                sasReadBlobUriOriginalPhoto, sasDeleteBlobUriOriginalPhoto, sasWriteBlobUriLargePhoto, sasWriteBlobUriMediumPhoto, sasWriteBlobUriThumbnailPhoto
            ));

            queue.AddMessage(message);
        }


        /// <summary>
        /// Puts a video conversion message in the videoconversion photoResizingQueue.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <param name="blobUriOriginalVideo"></param>
        private void QueueVideoConversionMessage(int mediaId, string blobUriOriginalVideo)
        {
            /* Getting the blob URI where we will store the converted video. */
            var blobUriConvertedVideo = GetBlobUriForConvertedVideo(blobUriOriginalVideo);

            /* Acquiring the appropriate Shared Access Signatures (SAS) allowing us to read, write and delete the blob URIs. */
            var sasReadBlobUriOriginalVideo = GetSasUriForBlobReadBL(blobUriOriginalVideo);
            var sasDeleteBlobUriOriginalVideo = GetSasUriForBlobDeleteBL(blobUriOriginalVideo);
            var sasWriteBlobUriConvertedVideo = GetSasUriForBlobWrite(blobUriConvertedVideo);

            /* Getting the file names, we need this when we store the files in local storage in the VideoConversionWorkerRole. */
            var originalVideoFileName = GetFileNameFromUri(blobUriOriginalVideo);
            var convertedVideoFileName = GetFileNameFromUri(blobUriConvertedVideo);

            /* Getting a reference to the videoconversion photoResizingQueue, where we will store the video conversion message. */
            var queue = queueStorage.GetQueueReference("videoconversion");

            /* In the message we store:
             * - The MediaID of the video.
             * - The blob URI which will point to the converted video.
             * - Various SAS URIs (Read, Delete, Write) which we need in the interaction with blob storage.
             * - The file name of the the original and the converted video, this is needed by the MediaManagerPro.dll 
             * video processing component (which we use for converting the videos in the 
             * VideoConversionWorkerRole), it has the parameters: SourceFile_Name and OutputFile_Name. */
            var message = new CloudQueueMessage(String.Format
            (
                "{0},{1},{2},{3},{4},{5},{6}",
                mediaId,
                blobUriConvertedVideo,
                sasReadBlobUriOriginalVideo, sasDeleteBlobUriOriginalVideo, sasWriteBlobUriConvertedVideo,
                originalVideoFileName, convertedVideoFileName
            ));

            queue.AddMessage(message);
        }
    }
}