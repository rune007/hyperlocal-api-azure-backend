using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class ContactInfoRequestDto
    {
        [DataMember]
        public int ContactInfoRequestID { get; set; }

        [DataMember]
        public int FromUserID { get; set; }

        [DataMember]
        public string FromUserName { get; set; }

        [DataMember]
        public string FromUserPhotoUri { get; set; }

        [DataMember]
        public int ToUserID { get; set; }

        [DataMember]
        public DateTime CreateDate { get; set; }

        [DataMember]
        public int NumberOfRequests { get; set; }

        [DataMember]
        public bool HasNextPageOfData { get; set; }
    }
}