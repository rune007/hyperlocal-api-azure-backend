using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class MessageDto
    {
        /// <summary>
        /// We use a string version of the receiver UserID as PartitionKey.
        /// </summary>
        [DataMember]
        public string PartitionKey { get; set; }

        [DataMember]
        public string ReceiverUserName { get; set; }

        [DataMember]
        public string ReceiverPhotoUri { get; set; }

        [DataMember]
        public string RowKey { get; set; }

        [DataMember]
        public int SenderUserID { get; set; }

        [DataMember]
        public string SenderUserName { get; set; }

        [DataMember]
        public string SenderPhotoUri { get; set; }

        [DataMember]
        public string Subject { get; set; }

        [DataMember]
        public string MessageBody { get; set; }

        [DataMember]
        public bool IsRead { get; set; }

        [DataMember]
        public bool DeletedBySender { get; set; }

        [DataMember]
        public bool DeletedByReceiver { get; set; }

        [DataMember]
        public DateTime CreateDate { get; set; }

        /// <summary>
        /// This is used with paging, and indicates whether there is more data to fetch when paging through data with server side data paging.
        /// </summary>
        [DataMember]
        public bool HasNextPageOfData { get; set; }
    }
}