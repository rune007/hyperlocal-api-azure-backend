using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class NewsItemVideoDto
    {
        [DataMember]
        public int MediaID { get; set; }

        [DataMember]
        public int NewsItemID { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string BlobUri { get; set; }

        [DataMember]
        public DateTime CreateUpdateDate { get; set; }
    }
}