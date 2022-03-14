using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.AzureTableStorage;
using HLServiceRole.DataTransferObjects;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        /// <summary>
        /// We have a generic DeleteMessage() method, used from both inbox and oubox, this is enabled by the enum MessageOwner.
        /// </summary>
        public enum MessageOwner { Sender, Receiver };


        public bool SendMessageBL(int receiverUserId, int senderUserId, string subject, string messageBody)
        {
            try
            {
                azureTableDataSource.SendMessage(receiverUserId, senderUserId, subject, messageBody);
                /* Alerting the receiver of the message. */
                AlertUserOnMessageReceived(receiverUserId, senderUserId, messageBody);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MessageBL.SendMessageBL(): " + ex.ToString());
                return false;
            }
        }


        public List<MessageDto> GetInboxContentBL(int userId, int pageSize, int pageNumber)
        {
            try
            {
                var entities = azureTableDataSource.GetInboxContent(userId);

                var messageDtos = new List<MessageDto>();

                if (entities != null)
                {
                    foreach (var e in entities)
                    {
                        messageDtos.Add
                        (
                            new MessageDto()
                            {
                                PartitionKey = e.PartitionKey,
                                RowKey = e.RowKey,
                                SenderUserID = e.SenderUserID,
                                SenderUserName = GetUserName(e.SenderUserID),
                                SenderPhotoUri = GetUserPhotoThumbnail(e.SenderUserID),
                                Subject = e.Subject,
                                MessageBody = e.MessageBody,
                                IsRead = e.IsRead,
                                DeletedBySender = e.DeletedBySender,
                                DeletedByReceiver = e.DeletedByReceiver,
                                CreateDate = e.CreateDate
                            }
                        );
                    }
                    /* In the below lines I am probing for whether there will be a next page of data with Count().
                     Then I am doing pagination with Skip() and Take(). These operations should really have been
                     done against the table entities themselves in class HlTableDataSource.cs, but it turned out
                     difficult to use these linq operators against the table entities, therefore I am doing it 
                     against the MessageDto List instead. */

                    /* Probing for whether there will be a next page of data beyond the current page. */
                    var numberOfMessages = messageDtos.Count();
                    bool hasNextPageOfData = numberOfMessages > (pageNumber * pageSize) ? true : false;
                    /* Updating the HasNextPageOfData field in MessageDto. */
                    foreach (var m in messageDtos)
                    {
                        m.HasNextPageOfData = hasNextPageOfData;
                    }

                    var messageDtosQueryable = messageDtos.AsQueryable();
                    /*Pagination */
                    messageDtosQueryable = messageDtosQueryable.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                    messageDtos = messageDtosQueryable.ToList();
                    return messageDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MessageBL.GetInboxContentBL(): " + ex.ToString());
                return null;
            }
        }


        public List<MessageDto> GetOutboxContentBL(int userId, int pageSize, int pageNumber)
        {
            try
            {
                var entities = azureTableDataSource.GetOutboxContent(userId);

                var messageDtos = new List<MessageDto>();

                if (entities != null)
                {
                    foreach (var e in entities)
                    {
                        messageDtos.Add
                        (
                            new MessageDto()
                            {
                                PartitionKey = e.PartitionKey,
                                ReceiverUserName = GetUserName(Convert.ToInt32(e.PartitionKey)),
                                ReceiverPhotoUri = GetUserPhotoThumbnail(Convert.ToInt32(e.PartitionKey)),
                                RowKey = e.RowKey,
                                SenderUserID = e.SenderUserID,
                                SenderUserName = GetUserName(e.SenderUserID),
                                SenderPhotoUri = GetUserPhotoThumbnail(e.SenderUserID),
                                Subject = e.Subject,
                                MessageBody = e.MessageBody,
                                IsRead = e.IsRead,
                                DeletedBySender = e.DeletedBySender,
                                DeletedByReceiver = e.DeletedByReceiver,
                                CreateDate = e.CreateDate
                            }
                        );
                    }
                    /* In the below lines I am probing for whether there will be a next page of data with Count().
                     Then I am doing pagination with Skip() and Take(). These operations should really have been
                     done against the table entities themselves in class HlTableDataSource.cs, but it turned out
                     difficult to use these linq operators against the table entities, therefore I am doing it 
                     against the MessageDto List instead. */

                    /* Probing for whether there will be a next page of data beyond the current page. */
                    var numberOfMessages = messageDtos.Count();
                    bool hasNextPageOfData = numberOfMessages > (pageNumber * pageSize) ? true : false;
                    /* Updating the HasNextPageOfData field in MessageDto. */
                    foreach (var m in messageDtos)
                    {
                        m.HasNextPageOfData = hasNextPageOfData;
                    }

                    var messageDtosQueryable = messageDtos.AsQueryable();
                    /* Order by date. */
                    messageDtosQueryable = messageDtosQueryable.OrderByDescending(m => m.CreateDate);
                    /*Pagination */
                    messageDtosQueryable = messageDtosQueryable.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                    messageDtos = messageDtosQueryable.ToList();
                    return messageDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MessageBL.GetOutboxContentBL(): " + ex.ToString());
                return null;
            }
        }


        private void DeleteUsersOutbox(int userId)
        {
            try
            {
                var entities = azureTableDataSource.GetOutboxContent(userId);

                if (entities != null)
                {
                    foreach (var e in entities)
                    {
                        DeleteMessageBL(MessageOwner.Sender, e.PartitionKey, e.RowKey);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MessageBL.DeleteUsersOutbox(): " + ex.ToString());
            }
        }



        private void DeleteUsersInbox(int userId)
        {
            try
            {
                var entities = azureTableDataSource.GetInboxContent(userId);

                if (entities != null)
                {
                    foreach (var e in entities)
                    {
                        DeleteMessageBL(MessageOwner.Receiver, e.PartitionKey, e.RowKey);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MessageBL.DeleteUsersInbox(): " + ex.ToString());
            }
        }


        public bool DeleteMessageBL(MessageOwner messageOwner, string partitionKey, string rowKey)
        {
            try
            {
                azureTableDataSource.DeleteMessage((HlTableDataSource.MessageOwner)messageOwner, partitionKey, rowKey);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MessageBL.DeleteMessageBL(): " + ex.ToString());
                return false;
            }
        }


        public MessageDto GetMessageBL(string partitionKey, string rowKey)
        {
            try
            {
                var entity = azureTableDataSource.GetMessage(partitionKey, rowKey);

                if (entity != null)
                {
                    var messageDto = new MessageDto()
                    {
                        PartitionKey = entity.PartitionKey,
                        ReceiverUserName = GetUserName(Convert.ToInt32(entity.PartitionKey)),
                        ReceiverPhotoUri = GetUserPhotoThumbnail(Convert.ToInt32(entity.PartitionKey)),
                        RowKey = entity.RowKey,
                        SenderUserID = entity.SenderUserID,
                        SenderUserName = GetUserName(entity.SenderUserID),
                        SenderPhotoUri = GetUserPhotoThumbnail(entity.SenderUserID),
                        Subject = entity.Subject,
                        MessageBody = entity.MessageBody,
                        IsRead = entity.IsRead,
                        DeletedBySender = entity.DeletedBySender,
                        DeletedByReceiver = entity.DeletedByReceiver,
                        CreateDate = entity.CreateDate
                    };
                    return messageDto;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MessageBL.GetMessageBL(): " + ex.ToString());
                return null;
            }
        }


        public int GetNumberOfUnreadMessagesBL(int receiverUserId)
        {
            try
            {
                var number = azureTableDataSource.GetNumberOfUnreadMessages(receiverUserId);

                if (number >= 0)
                {
                    return number;
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MessageBL.GetNumberOfUnreadMessagesBL(): " + ex.ToString());
                return -1;
            }
        }


        public bool MarkMessageAsReadBL(string partitionKey, string rowKey)
        {
            try
            {
                azureTableDataSource.MarkMessageAsRead(partitionKey, rowKey);
                return true;

            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in MessageBL.MarkMessageAsReadBL(): " + ex.ToString());
                return false;
            }
        }
    }
}