using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class CommentDto
    {
        /// <summary>
        /// PartionKey is the NewsItemID with a zero prepended. (E.g. NewsItemID: 45 -> PartitionKey: 045)
        /// The NewsItem is naturally the NewsItem which the Comment is a Comment to.
        /// </summary>
        [DataMember]
        public string PartitionKey { get; set; }

        [DataMember]
        public int NewsItemID { get; set; }

        [DataMember]
        public string RowKey { get; set; }

        [DataMember]
        public int PostedByUserID { get; set; }

        [DataMember]
        public string PostedByUserName { get; set; }

        [DataMember]
        public string CommentBody { get; set; }

        [DataMember]
        public DateTime CreateDate { get; set; }

        [DataMember]
        public string MediumSizeBlobUri { get; set; }

        [DataMember]
        public string ThumbnailBlobUri { get; set; }

        /// <summary>
        /// This is used with paging, and indicates whether there is more data to fetch when paging through data with server side data paging.
        /// </summary>
        [DataMember]
        public bool HasNextPageOfData { get; set; }
    }
}