using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]       
    public class UserAlertDto
    {
        [DataMember]
        public int UserID { get; set; }

        [DataMember]
        public bool AlertOnNews { get; set; }

        [DataMember]
        public bool AlertOnBreakingNews { get; set; }

        [DataMember]
        public bool AlertOnContactRequests { get; set; }

        [DataMember]
        public bool AlertOnMessages { get; set; }

        [DataMember]
        public bool AlertOnComments { get; set; }

        [DataMember]
        public bool AlertOnNotificationFromEditors { get; set; }

        [DataMember]
        public bool SendEmail { get; set; }

        [DataMember]
        public bool UsePushNotification { get; set; }
    }
}