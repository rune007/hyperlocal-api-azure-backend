using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class NewsItemPhotoDto
    {
        [DataMember]
        public int MediaID { get; set; }

        [DataMember]
        public int NewsItemID { get; set; }

        [DataMember]
        public string Caption { get; set; }

        [DataMember]
        public string BlobUri { get; set; }

        [DataMember]
        public string MediumSizeBlobUri { get; set; }

        [DataMember]
        public string ThumbnailBlobUri { get; set; }

        [DataMember]
        public DateTime CreateUpdateDate { get; set; }
    }
}