using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;

namespace HLServiceRole.AzureTableStorage
{
    public class MessageEntityModel : TableServiceEntity
    {
        public MessageEntityModel()
        {
        }


        public MessageEntityModel(int receiverUserId)
            // PartitionKey is UserID
            // RowKey is reversed DateTime.Now.Ticks, because we want the newest mails to be at beginning of the list.
            : base(receiverUserId.ToString(), string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid()))
        {
            CreateDate = DateTime.Now;
            IsRead = false;
            DeletedBySender = false;
            DeletedByReceiver = false;
        }

        public int SenderUserID { get; set; }
        public string Subject { get; set; }
        public string MessageBody { get; set; }
        public bool IsRead { get; set; }
        public bool DeletedBySender { get; set; }
        public bool DeletedByReceiver { get; set; }
        public DateTime CreateDate { get; set; }
    }
}