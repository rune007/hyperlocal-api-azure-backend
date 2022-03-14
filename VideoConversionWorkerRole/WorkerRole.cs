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
using VideoConversionWorkerRole.HLServiceReference;
using All4DotNet;
using System.IO;
using System.Collections;

namespace VideoConversionWorkerRole
{
    public class WorkerRole : RoleEntryPoint
    {
        /// <summary>
        /// Used to transport a video to be converted from the WCF role to the VideoConversionWorkerRole
        /// </summary>
        private CloudQueue videoConversionQueue;

        /// <summary>
        /// Used to transport the MediaID of the converted video back to the WCF role to signal that the video conversion is complete.
        /// </summary>
        private CloudQueue videoConversionStatusQueue;


        public override void Run()
        {
            Trace.WriteLine("VideoConversionWorkerRole entry point called");

            while (true)
            {
                try
                {
                    /* Retrieve a new message from the videoConversionQueue. */
                    CloudQueueMessage msg = videoConversionQueue.GetMessage();
                    if (msg != null)
                    {
                        /* Parse message retrieved from videoConversionQueue. */
                        var messageParts = msg.AsString.Split(new char[] { ',' });

                        var mediaId = messageParts[0];

                        /* Blob URIs where we will store the converted video. */
                        var blobUriConvertedVideo = messageParts[1];

                        /* Shared Access Signatures (SAS) allowing us to read, write and delete the blob URIs. */
                        var sasReadBlobUriOriginalVideo = messageParts[2];
                        var sasDeleteBlobUriOriginalVideo = messageParts[3];
                        var sasWriteBlobUriConvertedVideo = messageParts[4];

                        /* The file name of the original and the converted video, this is needed by the MediaManagerPro.dll video processing component 
                         * (which we use for converting the videos), it has the parameters: SourceFile_Name OutputFile_Name. */
                        var originalVideoFileName = messageParts[5];
                        var convertedVideoFileName = messageParts[6];

                        Trace.TraceInformation("Started processing video with MediaID: '{0}'.", mediaId.ToString());

                        /* Converting the video. */
                        ConvertTo_MP4(sasReadBlobUriOriginalVideo, sasWriteBlobUriConvertedVideo, originalVideoFileName, convertedVideoFileName);

                        /* Report back to HLService the converted video blob URI + Telling the video conversion has finished. */
                        using (var WS = new HLServiceClient())
                        {
                            try
                            {
                                WS.UpdateConvertedVideoBlobUri(Convert.ToInt32(mediaId), blobUriConvertedVideo);

                                /* Putting a message with the MediaID of the converted video in the videoConversionStatusQueue to 
                                 * signal back to the WCF application that the conversion of the video is finished. */
                                QueueVideoConversionStatusMessage(Convert.ToInt32(mediaId));
                            }
                            catch (Exception ex)
                            {
                                Trace.TraceError("There was a problem with HLService: " + ex.ToString());
                            }
                        }

                        /* Deleting the orginal video blob. */
                        try
                        {
                            CloudBlob orginalVideoDeleteBlob = new CloudBlob(sasDeleteBlobUriOriginalVideo);
                            orginalVideoDeleteBlob.DeleteIfExists();
                        }
                        catch (Exception ex)
                        {
                            Trace.TraceError("There was a problem with deleting the original video blob: " + ex.ToString());
                        }

                        /* Remove message from videoConversionQueue. */
                        videoConversionQueue.DeleteMessage(msg);

                        Trace.TraceInformation("Completed processing video with MediaID: '{0}'.", mediaId.ToString());
                    }
                    else
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                catch (StorageClientException e)
                {
                    Trace.TraceError("Exception when processing videoConversionQueue item. Message: '{0}'", e.Message);
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

            // Initialize videoConversionQueue storage.
            videoConversionQueue = queueStorage.GetQueueReference("videoconversion");
            Trace.Write("VideoConversionWorkerRole creating videoConversionQueue...");

            // Initialize videoConversionStatusQueue storage.
            videoConversionStatusQueue = queueStorage.GetQueueReference("videoconversionstatus");
            Trace.Write("VideoConversionWorkerRole creating videoConversionStatsQueue...");

            bool storageInitialized = false;
            while (!storageInitialized)
            {
                try
                {
                    // Create the videoConversionQueue.
                    videoConversionQueue.CreateIfNotExist();
                    // Create the videoConversionStatusQueue.
                    videoConversionStatusQueue.CreateIfNotExist();

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


        private void ConvertTo_MP4(string sasReadBlobUriOriginalVideo, string sasWriteBlobUriConvertedVideo, string originalVideoFileName, string convertedVideoFileName)
        {
            /* Declare object of class MediaManagerPro. */
            MediaManagerPro oMediaManagerPro = new MediaManagerPro();

            /* Gets a reference to the local storage, where we will temporary store the video files prior, and after conversion. 
             * Thus we will first temporary store the source video file, that is the original video file, which we download from blob. 
             * After the video conversion we will temporary store the converted video file, before uploading it to blob storage. */
            LocalResource localResource = RoleEnvironment.GetLocalResource("VideoLocalStorage");

            /* Gets an absolute path in order to access the ffmpeg.exe file. */
            var strRootPath = GetRootPath();

            /* Set the directory path info for ffmpeg file. */
            oMediaManagerPro.FFMPEG_Path = strRootPath + "\\ffmpeg\\ffmpeg.exe";

            /* Downloads the original video file from blob to local storage. */
            using (FileStream fs = File.Create(localResource.RootPath + "\\" + originalVideoFileName))
            {
                try
                {
                    var readBlob = new CloudBlob(sasReadBlobUriOriginalVideo);
                    readBlob.DownloadToStream(fs);
                    Trace.WriteLine("Downloadeded source file from blob.");
                }
                catch (Exception ex)
                {
                    Trace.TraceError("There was a problem downloading source file from blob. " + ex.ToString());
                }
            }

            /* Set source file info. */
            oMediaManagerPro.SourceFile_Path = localResource.RootPath;
            oMediaManagerPro.SourceFile_Name = originalVideoFileName;

            /* Set output file info. */
            oMediaManagerPro.OutputFile_Path = localResource.RootPath;
            oMediaManagerPro.OutputFile_Name = convertedVideoFileName;

            /* Call ConvertTo_MP4 method. */
            MediaInfo oMediaInfo = oMediaManagerPro.ConvertTo_MP4();

            /* Retrieve eventual video conversion error information. */
            if (oMediaInfo.Error_Code > 0)
            {
                Trace.TraceError(":: Video processing failed ::<br/>Error code: " + oMediaInfo.Error_Code + "<br />Error Message: " + oMediaInfo.Error_Message);
                return;
            }

            Trace.WriteLine("Video Converted", "Information");

            /* Uploads the converted video from local storage to blob storage. */
            using (FileStream fs = File.OpenRead(localResource.RootPath + "\\" + convertedVideoFileName))
            {
                try
                {
                    var writeBlob = new CloudBlob(sasWriteBlobUriConvertedVideo);
                    writeBlob.UploadFromStream(fs);
                    Trace.WriteLine("Uploaded converted file to blob");
                }
                catch (Exception ex)
                {
                    Trace.TraceError("There was a problem uploading converted file to blob: " + ex.ToString());
                }
            }

            /* Deleting the video files from local storage, they where just stored there temporarily because the MediaManagerPro component 
             * does not have a blob API, but needs to be accessed through a traditional file system. */
            try
            {
                File.Delete(localResource.RootPath + "\\" + originalVideoFileName);
                File.Delete(localResource.RootPath + "\\" + convertedVideoFileName);
                Trace.WriteLine("Video files deleted from local storage.");
            }
            catch (Exception ex)
            {
                Trace.TraceError("There was a problem deleting the video files from local storage: " + ex.ToString());
            }
        }


        /// <summary>
        /// To signal back to the WCF application that the video conversion has finished we put the MediaID
        /// of the converted video into a message in the videoConversionStatusQueue.
        /// </summary>
        /// <param name="mediaId">MediaID of the resized photo.</param>
        private void QueueVideoConversionStatusMessage(int mediaId)
        {
            var message = new CloudQueueMessage(mediaId.ToString());
            videoConversionStatusQueue.AddMessage(message);
        }


        /// <summary>
        /// Gets an absolute root path in order to access our ffmpeg.exe file. We need this method because when the application run in the
        /// Azure Emulator, the root path looks like:
        /// C:\Users\a\Documents\Visual Studio 2010\Projects\VideoConversionAzure\VideoConversionAzure\bin\Debug\VideoConversionAzure.csx\roles\VideoConversionWorkerRole
        /// This is not a valid path to access the file, instead we need a root path like:
        /// C:\Users\a\Documents\Visual Studio 2010\Projects\VideoConversionAzure\VideoConversionWorkerRole
        /// This method gets that root path.
        /// </summary>
        /// <returns></returns>
        private string GetRootPath()
        {
            string appRootDir = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory);

            var pathParts = appRootDir.Split((new char[] { '\\' }));

            var indexLastPathPart = pathParts.Length - 1;

            ArrayList newPathParts = new ArrayList();

            /* Copying contents of Array pathParts to ArrayList newPathParts. */
            for (var i = 0; i <= indexLastPathPart; i++)
            {
                newPathParts.Add(pathParts[i]);
            }

            /* Removing path parts from the "debugging path" in order to assemble an absolute path root, in order to access our files. */
            for (var i = indexLastPathPart - 1; i > indexLastPathPart - 6; i--)
            {
                newPathParts.Remove(pathParts[i]);
            }

            string strRootPath = "";

            /* Creating an absolute path root where the "debugging path" path parts are removed. */
            for (var i = 0; i < newPathParts.Count - 1; i++)
            {
                strRootPath += newPathParts[i] + "\\";
            }
            strRootPath += newPathParts[newPathParts.Count - 1];

            return strRootPath;
        }
    }
}
