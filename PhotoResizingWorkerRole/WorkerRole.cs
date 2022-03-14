using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Threading;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;
using Microsoft.WindowsAzure.StorageClient;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.IO;
using PhotoResizingWorkerRole.HLServiceReference;

namespace PhotoResizingWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        /// <summary>
        /// Into the photoResizingQueue we dispatch messages which contains all that the PhotoResizingWorkerRole
        /// need to resize the photo and generate thumbnail, medium and large photos out of the original photo. 
        /// Thus the photoResizingQueue is a means of communication from the HLServiceRole to the PhotoResizingWorkerRole. 
        /// That is from the WCF application to the worker role.
        /// </summary>
        private CloudQueue photoResizingQueue;

        /// <summary>
        /// The photo resizing process is initiated in the various save-photo-methods that I have in class HLServiceRole/MediaBL:
        /// SaveNewsItemPhoto(), SaveUserPhoto(), SaveCommunityPhoto(), SaveAssignmentPhoto(). We need a way for the 
        /// PhotoResizingWorkerRole to communicate back to these methods that it has finished the resizing of the 
        /// photo, so that we can go to the Details view out in the client and see the resized photo. The photoResizingStatusQueue
        /// is the means of communication for this task. All media in my system have a unique MediaID. When a save-photo-method 
        /// ships off a message to resize the photo it remembers this MediaID. When the PhotoResizingWorkerRole has
        /// finished resizing of the photo it puts the MediaID of the resized photo into a message which is put into the photoResizingStatusQueue.
        /// The save-photo-methods gets these messages from photoResizingStatusQueue. When a particular save-photo-method gets a particular
        /// MediaID, it knows that its photo have been resized and will then let the client display the resized photo.
        /// Say for example that the method SaveAssignmentPhoto() ships off a photo to be resized with the MediaID 737, when SaveAssignmentPhoto()
        /// later reads a message from the photoResizingStatusQueue with the value of 737, it knows that its photo have been resized. */
        /// </summary>
        private CloudQueue photoResizingStatusQueue;

        /// <summary>
        /// We resize the photos into three sizes: Large, Medium and Thumbnail.
        /// </summary>
        public enum PhotoSize { Large, Medium, Thumbnail };


        public override void Run()
        {
            Trace.WriteLine("PhotoResizingWorkerRole entry point called");

            while (true)
            {
                try
                {
                    /* Retrieve a new message from the photoResizingQueue. */
                    CloudQueueMessage msg = photoResizingQueue.GetMessage();
                    if (msg != null)
                    {
                        /* Parse message retrieved from photoResizingQueue. */
                        var messageParts = msg.AsString.Split(new char[] { ',' });

                        var mediaId = messageParts[0];

                        /* Blob URIs where we will store the resized photos. */
                        var blobUriLargePhoto = messageParts[1];
                        var blobUriMediumPhoto = messageParts[2];
                        var blobUriThumbnailPhoto = messageParts[3];

                        /* Shared Access Signatures (SAS) allowing us to read, write and delete the blob URIs. */
                        var sasReadBlobUriOriginalPhoto = messageParts[4];
                        var sasDeleteBlobUriOriginalPhoto = messageParts[5];
                        var sasWriteBlobUriLargePhoto = messageParts[6];
                        var sasWriteBlobUriMediumPhoto = messageParts[7];
                        var sasWriteBlobUriThumbnailPhoto = messageParts[8];

                        Trace.TraceInformation("Started processing photo with MediaID: '{0}'.", mediaId.ToString());

                        /* Resizing the photo. */
                        ResizePhoto(sasReadBlobUriOriginalPhoto, sasWriteBlobUriThumbnailPhoto, PhotoSize.Thumbnail);
                        ResizePhoto(sasReadBlobUriOriginalPhoto, sasWriteBlobUriMediumPhoto, PhotoSize.Medium);
                        ResizePhoto(sasReadBlobUriOriginalPhoto, sasWriteBlobUriLargePhoto, PhotoSize.Large);

                        /* Report back to HLService the resized photo blob URIs + Telling the photo conversion has finished. */
                        using (var WS = new HLServiceClient())
                        {
                            try
                            {
                                WS.UpdateResizedPhotoBlobUris(Convert.ToInt32(mediaId), blobUriLargePhoto, blobUriMediumPhoto, blobUriThumbnailPhoto);

                                /* Putting a message with the MediaID of the resized photo in the PhotoResizingStatusQueue to signal back to the 
                                 WCF application that the photo resizing of the photo is finished. */
                                QueuePhotoResizingStatusMessage(Convert.ToInt32(mediaId));
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("There was a problem with HLService: " + ex.ToString());
                            }
                        }

                        /* Deleting the orginal photo blob. */
                        CloudBlob orginalPhotoDeleteBlob = new CloudBlob(sasDeleteBlobUriOriginalPhoto);
                        orginalPhotoDeleteBlob.DeleteIfExists();

                        /* Remove message from photoResizingQueue. */
                        photoResizingQueue.DeleteMessage(msg);

                        Trace.TraceInformation("Completed processing photo with MediaID: '{0}'.", mediaId.ToString());
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (StorageClientException e)
                {
                    Trace.TraceError("Exception when processing photoResizingQueue item. Message: '{0}'", e.Message);
                    System.Threading.Thread.Sleep(5000);
                }
            }
        }


        /// <summary>
        /// This is basically a cloud storage initialization method.
        /// </summary>
        public override bool OnStart()
        {
            // Set the maximum number of concurrent connections 
            ServicePointManager.DefaultConnectionLimit = 12;

            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            // Read storage account configuration settings
            CloudStorageAccount.SetConfigurationSettingPublisher((configName, configSetter) =>
            {
                configSetter(RoleEnvironment.GetConfigurationSettingValue(configName));
            });
            var storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
         
            CloudQueueClient queueStorage = storageAccount.CreateCloudQueueClient();

            // Initialize photoResizingQueue storage. 
            photoResizingQueue = queueStorage.GetQueueReference("photoresizing");
            Trace.TraceInformation("PhotoResizing creating photoResizingQueue...");

            // Initialize photoResizingStatusQueue storage. 
            photoResizingStatusQueue = queueStorage.GetQueueReference("photoresizingstatus");
            Trace.TraceInformation("PhotoResizing creating photoResizingStatusQueue...");

            bool storageInitialized = false;
            while (!storageInitialized)
            {
                try
                {
                    // Create the photoResizingQueue.
                    photoResizingQueue.CreateIfNotExist();
                    // Create the photoResizingStatusQueue.
                    photoResizingStatusQueue.CreateIfNotExist();

                    storageInitialized = true;
                }
                catch (StorageClientException e)
                {
                    if (e.ErrorCode == StorageErrorCode.TransportError)
                    {
                        Trace.TraceError("Storage services initialization failure. "
                          + "Check your storage account configuration settings. If running locally, "
                          + "ensure that the Development Storage service is running. Message: '{0}'", e.Message);
                        System.Threading.Thread.Sleep(5000);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return base.OnStart();
        }


        private void ResizePhoto(string sasReadBlobUri, string sasWriteBlobUri, PhotoSize photoSize)
        {
            try
            {
                /* Creating storage clients blobs from the SAS URIs in order to access the actual blobs. */
                CloudBlob inputBlob = new CloudBlob(sasReadBlobUri);
                CloudBlob outputBlob = new CloudBlob(sasWriteBlobUri);

                using (BlobStream input = inputBlob.OpenRead())
                using (BlobStream output = outputBlob.OpenWrite())
                {
                    int size = 0;

                    /* Setting the size of the photo. */
                    switch (photoSize)
                    {
                        case PhotoSize.Large:
                            size = 500;
                            break;
                        case PhotoSize.Medium:
                            size = 300;
                            break;
                        case PhotoSize.Thumbnail:
                            size = 64;
                            break;
                    }

                    int width = size;
                    int height = size;

                    /* Doing the actual resizing of the photo. */
                    var originalImage = new Bitmap(input);

                    if (originalImage.Width > originalImage.Height)
                    {
                        height = size * originalImage.Height / originalImage.Width;
                    }
                    else
                    {
                        width = size * originalImage.Width / originalImage.Height;
                    }

                    var resizedImage = new Bitmap(width, height);

                    using (Graphics graphics = Graphics.FromImage(resizedImage))
                    {
                        graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        graphics.SmoothingMode = SmoothingMode.AntiAlias;
                        graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        graphics.DrawImage(originalImage, 0, 0, width, height);
                    }
                    resizedImage.Save(output, ImageFormat.Jpeg);

                    /* Commit the blob. */
                    output.Commit();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("There was a problem with ResizePhoto(): " + ex.ToString());
            }
        }


        /// <summary>
        /// To signal back to the WCF application that the photo resizing has finished we put the MediaID
        /// of the resized photo into a message in the photoResizingStatusQueue.
        /// </summary>
        /// <param name="mediaId">MediaID of the resized photo.</param>
        private void QueuePhotoResizingStatusMessage(int mediaId)
        {
            var message = new CloudQueueMessage(mediaId.ToString());
            photoResizingStatusQueue.AddMessage(message);
        }
    }
}
