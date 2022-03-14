using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.IO;
using System.Net;
using HLServiceRole.AzureTableStorage;
using HLServiceRole.EntityFramework;
using Microsoft.WindowsAzure.StorageClient;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        private static bool storageInitialized = false;
        private static object gate = new Object();
        private static CloudQueueClient queueStorage;

        /// <summary>
        /// Entity Framework
        /// </summary>
        HLDBEntities entityFramework = new HLDBEntities();

        /// <summary>
        /// This class exposes all the methods to read and write data stored in Azure table storage.
        /// </summary>
        HlTableDataSource azureTableDataSource = new HlTableDataSource();


        public BusinessLogic()
        {
            InitializeQueueStorage();
        }


        /// <summary>
        /// Initializing the the Azure Queue Storage
        /// The system uses 4 queues:
        /// - photoResizingQueue - Ships off photo to be resized - Flow: WCF role -> PhotoResizingWorkerRole.
        /// - videoConversionStatusQueue - Signals that photo resizing is finished - Flow: PhotoResizingWorkerRole -> WCF role.
        /// - videoConversionQueue - Ships off video to be converted - Flow: WCF role -> VideoConversionWorkerRole.
        /// - videoConversionStatusQueue - Signals that video conversion is finished - Flow: VideoConversionWorkerRole -> WCF role.
        /// </summary>
        private void InitializeQueueStorage()
        {
            if (storageInitialized)
            {
                return;
            }

            lock (gate)
            {
                if (storageInitialized)
                {
                    return;
                }
                try
                {
                    /* Read account configuration settings. */
                    var storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");

                    /* Creating a CloudQueueClient. */
                    queueStorage = storageAccount.CreateCloudQueueClient();

                    /* Into the photoResizingQueue we dispatch messages which contains all that the PhotoResizingWorkerRole
                     need to resize the photo and generate thumbnail, medium and large photos out of the original photo. 
                     Thus the photoResizingQueue is a means of communication from the HLServiceRole to the PhotoResizingWorkerRole. 
                     That is from the WCF application to the worker role. */
                    CloudQueue photoResizingQueue = queueStorage.GetQueueReference("photoresizing");
                    photoResizingQueue.CreateIfNotExist();

                    /* The photo resizing process is initiated in the various save-photo-methods that I have in class MediaBL:
                     * SaveNewsItemPhoto(), SaveUserPhoto(), SaveCommunityPhoto(), SaveAssignmentPhoto(). We need a way for the 
                     * PhotoResizingWorkerRole to communicate back to these methods that it has finished the resizing of the 
                     * photo, so that we can go to the Details view out in the client and see the resized photo. The videoConversionStatusQueue
                     * is the means of communication for this task. All media in my system have a unique MediaID. When a save-photo-method 
                     * ships off a message to resize the photo it remembers this MediaID. When the PhotoResizingWorkerRole has
                     * finished resizing of the photo it puts the MediaID of the resized photo into a message which is put into the videoConversionStatusQueue.
                     * The save-photo-methods gets these messages from videoConversionStatusQueue. When a particular save-photo-method gets a particular
                     * MediaID, it knows that its photo have been resized and will then let the client display the resized photo.
                     * Say for example that the method SaveAssignmentPhoto() ships off a photo to be resized with the MediaID 737, when SaveAssignmentPhoto()
                     * later reads a message from the videoConversionStatusQueue with the value of 737, it knows that its photo have been resized. */
                    CloudQueue photoResizingStatusQueue = queueStorage.GetQueueReference("photoresizingstatus");
                    photoResizingStatusQueue.CreateIfNotExist();

                    CloudQueue videoConversionQueue = queueStorage.GetQueueReference("videoconversion");
                    videoConversionQueue.CreateIfNotExist();

                    CloudQueue videoConversionStatusQueue = queueStorage.GetQueueReference("videoconversionstatus");
                    videoConversionStatusQueue.CreateIfNotExist();
                }
                catch (WebException)
                {
                    throw new WebException("Storage services initialization failure. "
                        + "Check your storage account configuration settings. If running locally, "
                        + "ensure that the Development Storage service is running.");
                }
                storageInitialized = true;
            }
        }
    }
}