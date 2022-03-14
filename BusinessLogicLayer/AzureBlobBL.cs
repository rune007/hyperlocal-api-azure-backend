using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.DataTransferObjects;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using HLServiceRole.EntityFramework;
using System.Diagnostics;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        /// <summary>
        /// Media used in conjunction with different objects: NewsItems, Users, Communities, Assignment
        /// are stored in different blob containers: newsitemphoto, userphoto, newsitemvideo, communityphoto, etc.
        /// </summary>
        public enum MediaUsage { News, User, Community, Assignment };

        /// <summary>
        /// Our media files boils down to basically two types: Photo and Video.
        /// </summary>
        public enum MediaType { Photo, Video };

        /// <summary>
        /// We resize the photos into three sizes: Large, Medium and Thumbnail.
        /// </summary>
        public enum PhotoSize { Large, Medium, Thumbnail };


        /// <summary>
        /// Gets a SAS URI to write to a blob for 15 minutes. This method is used by the clients when they upload media files.
        /// </summary>
        public string GetSasUriForBlobWriteBL(MediaUsage mediaUsage, string fileName)
        {
            try
            {
                var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;

                var blobClient = cloudStorageAccount.CreateCloudBlobClient();

                var containerName = GetBlobContainerNameForMedia(fileName, mediaUsage);

                var uniqueFileName = GetUniqueFileName(fileName);

                var container = blobClient.GetContainerReference(containerName);

                container.CreateIfNotExist();

                var blob = container.GetBlobReference(uniqueFileName);

                var sas = blob.GetSharedAccessSignature(new SharedAccessPolicy()
                {
                    Permissions = SharedAccessPermissions.Write,
                    SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(15)
                });
                return blob.Uri.AbsoluteUri + sas;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.GetSasUriForBlobWriteBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Returns a SAS to write, expires after 30 minutes. This method is used in conjunction with the PhotoResizingWorkerRole, to get SAS URIs for the resized photos.
        /// </summary>
        /// <param name="blobUri"></param>
        /// <returns></returns>
        public string GetSasUriForBlobWrite(string blobUri)
        {
            try
            {
                var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                var blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlob blob = null;

                if (blobUri != string.Empty)
                {
                    blob = blobClient.GetBlobReference(blobUri);

                    var sas = blob.GetSharedAccessSignature
                    (
                        new SharedAccessPolicy()
                        {
                            Permissions = SharedAccessPermissions.Write,
                            SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(30)
                        }
                    );
                    return blob.Uri.AbsoluteUri + sas;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.GetSasUriForBlobWrite(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Returns a SAS URI for read a blob which expires after 50 minutes.
        /// </summary>
        public string GetSasUriForBlobReadBL(string blobUri)
        {
            try
            {
                var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                var blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlob blob = null;

                if (blobUri != string.Empty)
                {
                    blob = blobClient.GetBlobReference(blobUri);

                    var sas = blob.GetSharedAccessSignature
                    (
                        new SharedAccessPolicy()
                        {
                            Permissions = SharedAccessPermissions.Read,
                            SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(50)
                        }
                    );
                    return blob.Uri.AbsoluteUri + sas;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.GetSasUriForBlobReadBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Returns a SAS URI for deleting a blob which expires after 30 minutes.
        /// </summary>
        private string GetSasUriForBlobDeleteBL(string blobUri)
        {
            try
            {
                var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                var blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlob blob = null;

                if (blobUri != string.Empty)
                {
                    blob = blobClient.GetBlobReference(blobUri);

                    var sas = blob.GetSharedAccessSignature
                    (
                        new SharedAccessPolicy()
                        {
                            Permissions = SharedAccessPermissions.Delete,
                            SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(30)
                        }
                    );
                    return blob.Uri.AbsoluteUri + sas;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.GetSasUriForBlobDeleteBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the SharedAccessPermissions.Delete to a blob and deletes it.
        /// </summary>
        private bool DeleteBlob(string blobUri)
        {
            try
            {
                var cloudStorageAccount = CloudStorageAccount.DevelopmentStorageAccount;
                var blobClient = cloudStorageAccount.CreateCloudBlobClient();
                CloudBlob blob = null;

                if (blobUri != string.Empty)
                {
                    blob = blobClient.GetBlobReference(blobUri);

                    var sas = blob.GetSharedAccessSignature
                    (
                        new SharedAccessPolicy()
                        {
                            Permissions = SharedAccessPermissions.Delete,
                            SharedAccessExpiryTime = DateTime.UtcNow + TimeSpan.FromMinutes(30)
                        }
                    );
                    var blobToDelete = new CloudBlob(blob.Uri.AbsoluteUri + sas);
                    blobToDelete.DeleteIfExists();
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.DeleteBlob(): " + ex.ToString());
                return false;
            }
        }


        #region Phone Methods


        public bool SaveImageBL(int salesItemId, string contentType, byte[] photo)
        {
            string imageBlobUri = this.SaveImageAsBlob
            (
                Guid.NewGuid().ToString() + ".jpg",
                contentType,
                photo
            );

            /* Calls the SaveImageUri() method in SalesItemsBL.cs to persist the imageBlobUri in the database. */
            //this.SaveImageUri(salesItemId, imageBlobUri);

            SaveMediaBL(salesItemId, imageBlobUri, MediaUsage.News);

            return true;
        }


        ///* Deletes the image from blob storage. */
        public bool DeleteImageBlobBL(string imageBlobUri)
        {
            var blob = this.GetBlobContainerForPhoto().GetBlobReference(imageBlobUri);
            blob.DeleteIfExists();
            return true;
        }


        // Create a blob in container and upload image bytes to it
        private string SaveImageAsBlob(string name, string contentType, byte[] data)
        {
            this.EnsureContainerExists();

            // Gets a reference to a blob with a particular name in the CloudBlobContainer
            var blob = this.GetBlobContainerForPhoto().GetBlobReference(name);

            // Set the content-type value for the blob.
            blob.Properties.ContentType = contentType;

            // Upload the array of bytes to the blob.
            blob.UploadByteArray(data);

            // Get the blob URI of the image.
            string imageBlobUri = Convert.ToString(blob.Uri);

            return imageBlobUri;
        }

        private void EnsureContainerExists()
        {
            var container = GetBlobContainerForPhoto();
            container.CreateIfNotExist();

            var permissions = container.GetPermissions();
            permissions.PublicAccess = BlobContainerPublicAccessType.Container;
            container.SetPermissions(permissions);
        }


        private CloudBlobContainer GetBlobContainerForPhoto()
        {
            // Get a handle on account, create a blob service client and get container proxy
            var account = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            var client = account.CreateCloudBlobClient();

            return client.GetContainerReference(RoleEnvironment.GetConfigurationSettingValue("PhotoContainerName"));
        }


        private CloudBlobContainer GetBlobContainerForVideo()
        {
            // Get a handle on account, create a blob service client and get container proxy
            var account = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");
            var client = account.CreateCloudBlobClient();

            return client.GetContainerReference(RoleEnvironment.GetConfigurationSettingValue("VideoContainerName"));
        }



        #endregion
    }
}