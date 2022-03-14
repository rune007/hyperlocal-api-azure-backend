using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HLServiceRole.DataTransferObjects
{
    /// <summary>
    /// Used to transport info in relation to User notifications (sending email or push notification).
    /// </summary>
    public class NotificationDto
    {
        public int UserID { get; set; }

        public string UserName { get; set; }

        public string Email { get; set; }

        public string FirstName { get; set; }

        public string PhoneNumber { get; set; }

        public string PushNotificationChannel { get; set; }

        public int NewsItemID { get; set; }

        public string Title { get; set; }

        public string CommentBody { get; set; }

        public string MessageBody { get; set; }

        public DateTime CreateUpdateDate { get; set; }

        public string CategoryName { get; set; }

        public int? AssignmentID { get; set; }

        public string AssignmentTitle { get; set; }

        public string Description { get; set; }

        public string AssignmentCenterAddress { get; set; }

        public int? AssignmentRadius { get; set; }

        public string PostedByUserName { get; set; }

        public string CommunityName { get; set; }

        public bool SendEmail { get; set; }

        public bool UsePushNotification { get; set; }
    }
}