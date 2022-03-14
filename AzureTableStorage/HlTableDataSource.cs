using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.StorageClient;
using System.Diagnostics;

namespace HLServiceRole.AzureTableStorage
{
    /// <summary>
    /// This class exposes all the methods to read and write data stored in Azure table storage.
    /// </summary>
    public class HlTableDataSource
    {
        private static CloudStorageAccount storageAccount;
        private MessageServiceContext messageServiceContext;
        private CommentServiceContext commentServiceContext;
        /// <summary>
        /// We have a generic DeleteMessage() method, used from both inbox and oubox, this is enabled by the enum MessageOwner.
        /// </summary>
        public enum MessageOwner { Sender, Receiver };


        static HlTableDataSource()
        {
            storageAccount = CloudStorageAccount.FromConfigurationSetting("DataConnectionString");

            CloudTableClient.CreateTablesFromModel(
                typeof(MessageServiceContext),
                storageAccount.TableEndpoint.AbsoluteUri,
                storageAccount.Credentials);

            CloudTableClient.CreateTablesFromModel(
                typeof(CommentServiceContext),
                storageAccount.TableEndpoint.AbsoluteUri,
                storageAccount.Credentials);
        }


        public HlTableDataSource()
        {
            this.messageServiceContext = new MessageServiceContext(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            this.messageServiceContext.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));

            this.commentServiceContext = new CommentServiceContext(storageAccount.TableEndpoint.AbsoluteUri, storageAccount.Credentials);
            this.commentServiceContext.RetryPolicy = RetryPolicies.Retry(3, TimeSpan.FromSeconds(1));
        }


        #region Comment Methods

        public CommentEntityModel CreateComment(int newsItemId, int userId, string commentBody)
        {
            try
            {
                var entity = new CommentEntityModel(newsItemId) { UserID = userId, CommentBody = commentBody };
                this.commentServiceContext.AddObject("HlTable", entity);
                this.commentServiceContext.SaveChanges();

                return entity;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.AddComment(): " + ex.ToString());
                return null;
            }
        }


        public void DeleteComment(string partitionKey, string rowKey)
        {
            try
            {
                var results = from c in this.commentServiceContext.HlTable
                              where c.PartitionKey == partitionKey
                              && c.RowKey == rowKey
                              select c;

                var entity = results.FirstOrDefault<CommentEntityModel>();
                this.commentServiceContext.DeleteObject(entity);
                this.commentServiceContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.DeleteComment(): " + ex.ToString());
            }
        }


        public IEnumerable<CommentEntityModel> GetCommentsOnNewsItem(int newsItemId, int pageSize, int pageNumber)
        {
            try
            {
                var results = from c in this.commentServiceContext.HlTable
                              /* Because we store both CommentEntityModels and MessageEntityModels in the same table
                             we prefix the CommentEntityModel.PartitionKey with a "0" in order to be able to distinguish
                             between the two kinds of entity models.*/
                              where c.PartitionKey == ("0" + newsItemId.ToString())
                              select c;
                var page = results.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                return results;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.GetCommentsOnNewsItem(): " + ex.ToString());
                return null;
            }
        }


        public void DeleteCommentsOnNewsItem(int newsItemId)
        {
            try
            {
                var results = from c in this.commentServiceContext.HlTable
                              /* Because we store both MessageEntityModels and CommentEntityModels in the same table
                               we need to prepend the CommentEntityModels with a "0" to ensure uniqueness of PartitionKeys.*/
                              where c.PartitionKey == ("0" + newsItemId.ToString())
                              select c;

                foreach (var r in results)
                {
                    this.commentServiceContext.DeleteObject(r);
                }
                this.commentServiceContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.DeleteCommentsOnNewsItem(): " + ex.ToString());
            }
        }


        public void DeleteCommentsPostedByUser(int userId)
        {
            try
            {
                var results = from c in this.commentServiceContext.HlTable
                              where c.UserID == userId
                              select c;

                foreach (var r in results)
                {
                    this.commentServiceContext.DeleteObject(r);
                }
                this.commentServiceContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.DeleteCommentsPostedByUser(): " + ex.ToString());
            }
        }


        /// <summary>
        /// Get Users to alert about a new Comment (Those who have already commented on the NewsItem. 
        /// We exclude the User who have posted the Comment (commentingUserID)
        /// </summary>
        public List<int> GetUsersWhoAlsoCommentedOnNewsItem(int commentedNewsItemId, int commentingUserId)
        {
            var results = from c in this.commentServiceContext.HlTable
                          /* Because we store both CommentEntityModels and MessageEntityModels in the same table
                           we prefix the CommentEntityModel.PartitionKey with a "0" in order to be able to distinguish
                           between the two kinds of entity models.*/
                          where c.PartitionKey == "0" + Convert.ToString(commentedNewsItemId)
                          where c.UserID != commentingUserId
                          select c;

            var resultList = new List<int>();

            foreach (var r in results)
            {
                resultList.Add(r.UserID);
            }

            /* Removing duplicates. */
            resultList = resultList.Distinct().ToList();

            return resultList.ToList();
        }

        #endregion


        #region Message Methods

        public void SendMessage(int receiverUserId, int senderUserId, string subject, string messageBody)
        {
            try
            {
                var entity = new MessageEntityModel(receiverUserId) { SenderUserID = senderUserId, Subject = subject, MessageBody = messageBody };
                this.messageServiceContext.AddObject("HlTable", entity);
                this.messageServiceContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.SendMessage(): " + ex.ToString());
            }
        }


        public MessageEntityModel GetMessage(string partitionKey, string rowKey)
        {
            try
            {
                var results = from m in this.messageServiceContext.HlTable
                              where m.PartitionKey == partitionKey
                              && m.RowKey == rowKey
                              select m;
                var entity = results.FirstOrDefault<MessageEntityModel>();
                return entity;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.GetMessage(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Deletes a message either for the receiver or the sender, by setting the fields DeletedByReceiver or DeletedBySender
        /// If the message has been deleted by the other part the message is deleted altogether from the system.
        /// </summary>
        public void DeleteMessage(MessageOwner messageOwner, string partitionKey, string rowKey)
        {
            try
            {
                var results = from m in this.messageServiceContext.HlTable
                              where m.PartitionKey == partitionKey
                              && m.RowKey == rowKey
                              select m;

                var entity = results.FirstOrDefault<MessageEntityModel>();
                // Checks whether the message has already been deleted by the receiver.
                var deletedByReceiver = entity.DeletedByReceiver;
                // Checks whether the message has already been deleted by the sender.
                var deletedBySender = entity.DeletedBySender;

                // If the message has already been deleted by the other part we delete it altogether.
                if (deletedByReceiver && messageOwner == MessageOwner.Sender)
                {
                    this.messageServiceContext.DeleteObject(entity);
                    this.messageServiceContext.SaveChanges();
                    deletedBySender = true;
                }
                if (deletedBySender && messageOwner == MessageOwner.Receiver)
                {
                    this.messageServiceContext.DeleteObject(entity);
                    this.messageServiceContext.SaveChanges();
                    deletedByReceiver = true;
                }

                // Otherwise we just set entity.DeletedBySender = true at the message owner.
                if (!deletedByReceiver && messageOwner == MessageOwner.Receiver)
                {
                    entity.DeletedByReceiver = true;
                    this.messageServiceContext.UpdateObject(entity);
                    this.messageServiceContext.SaveChanges();
                }
                if (!deletedBySender && messageOwner == MessageOwner.Sender)
                {
                    entity.DeletedBySender = true;
                    this.messageServiceContext.UpdateObject(entity);
                    this.messageServiceContext.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.DeleteMessage(): " + ex.ToString());
            }
        }


        public IEnumerable<MessageEntityModel> GetInboxContent(int receiverUserId)
        {
            try
            {
                var results = from m in this.messageServiceContext.HlTable
                              where m.PartitionKey == receiverUserId.ToString()
                              where m.DeletedByReceiver == false
                              select m;
                return results;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.GetInboxContent(): " + ex.ToString());
                return null;
            }
        }


        public IEnumerable<MessageEntityModel> GetOutboxContent(int senderUserId)
        {
            try
            {
                var results = from m in this.messageServiceContext.HlTable
                              where m.SenderUserID == senderUserId
                              where m.DeletedBySender == false
                              select m;
                return results;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.GetOutboxContent(): " + ex.ToString());
                return null;
            }
        }


        public void MarkMessageAsRead(string partitionKey, string rowKey)
        {
            try
            {
                var results = from m in this.messageServiceContext.HlTable
                              where m.PartitionKey == partitionKey
                              && m.RowKey == rowKey
                              select m;
                var entity = results.FirstOrDefault<MessageEntityModel>();
                entity.IsRead = true;
                this.messageServiceContext.UpdateObject(entity);
                this.messageServiceContext.SaveChanges();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.MarkMessageAsRead(): " + ex.ToString());
            }
        }


        public int GetNumberOfUnreadMessages(int receiverUserId)
        {
            try
            {
                var results = (from m in this.messageServiceContext.HlTable
                               where m.PartitionKey == receiverUserId.ToString()
                               where m.IsRead == false
                               select m).ToList();

                return results.Count();
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in HlTableDataSource.GetNumberOfUnreadMessages(): " + ex.ToString());
                return -1;
            }
        }

        #endregion
    }
}