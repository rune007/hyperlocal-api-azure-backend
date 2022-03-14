using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.DataTransferObjects;
using HLServiceRole.EntityFramework;
using System.Diagnostics;
using System.Text;
using System.Net.Mail;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        /// <summary>
        /// A generic method which handles all email notifications to the Users from the system.
        /// </summary>
        public void NotifyUserByEmail(NotificationDto dto, NotificationKind notificationKind)
        {
            MailMessage mailMessage = null;
            switch (notificationKind)
            {
                case NotificationKind.BreakingNews:
                    mailMessage = MakeBreakingNewsEmailMessage(dto);
                    break;
                case NotificationKind.News:
                    mailMessage = MakeNewsEmailMessage(dto);
                    break;
                case NotificationKind.ContactRequest:
                    mailMessage = MakeContactRequestEmailMessage(dto);
                    break;
                case NotificationKind.ContactRequestAccepted:
                    mailMessage = MakeContactRequestAcceptedEmailMessage(dto);
                    break;
                case NotificationKind.MessageReceived:
                    mailMessage = MakeMessageReceivedEmailMessage(dto);
                    break;
                case NotificationKind.Comment:
                    mailMessage = MakeNewCommentEmailMessage(dto);
                    break;
                case NotificationKind.NotificationFromEditors:
                    mailMessage = MakeNotificationFromEditorEmailMessage(dto);
                    break;
            }
            // Sends the email message.
            // The <smtp> configuration node in Web.config refers to a valid SMTP host or pickup directory
            // I am refering to a directory "email" on the C drive.
            using (var smtpClient = new SmtpClient())
            {
                smtpClient.Send(mailMessage);
            }
        }


        private MailMessage MakeBreakingNewsEmailMessage(NotificationDto dto)
        {
            var message = new StringBuilder();
            message.AppendLine(dto.CreateUpdateDate.ToString());
            message.AppendFormat("Hi {0}!\n", dto.FirstName);
            message.AppendLine("BREAKING NEWS");
            message.AppendLine(dto.Title);
            message.AppendFormat("From community \"{0}\" in category \"{1}\"\n", dto.CommunityName, dto.CategoryName);
            //if (dto.AssignmentTitle.Length > 0)
            if (dto.AssignmentID != null)
            {
                message.AppendFormat("On assignment \"{0}\"\n", dto.AssignmentTitle);
            }
            message.AppendFormat("Posted by {0}\n", dto.PostedByUserName);
            message.AppendLine("Go and check it out!");
            message.AppendFormat("http://127.0.0.2:81/News/{0}\n", dto.NewsItemID);
            message.AppendLine("Thanks");
            message.AppendLine("http://127.0.0.2:81/");
            return new MailMessage(
            "notification@hyperlocal.dk", // From
            dto.Email, // To
            dto.Title, // Subject
            message.ToString() // Body
            );
        }


        private MailMessage MakeContactRequestEmailMessage(NotificationDto dto)
        {
            var message = new StringBuilder();
            message.AppendLine(dto.CreateUpdateDate.ToString());
            message.AppendFormat("Hi {0}!\n", dto.FirstName);
            message.AppendLine("NEW CONTACT INFO REQUEST");
            message.AppendFormat("{0} wants to Share Contact Information With You!\n", dto.PostedByUserName);
            message.AppendLine("Please let them know what you say!");
            message.AppendLine("Go and check it out!");
            message.AppendLine("http://127.0.0.2:81/ContactInfo/Requests");
            message.AppendLine("Thanks");
            message.AppendLine("http://127.0.0.2:81/");
            return new MailMessage(
            "notification@hyperlocal.dk", // From
            dto.Email, // To
            dto.PostedByUserName + " wants to Share Contact Information With You!", // Subject
            message.ToString() // Body
            );
        }


        private MailMessage MakeContactRequestAcceptedEmailMessage(NotificationDto dto)
        {
            var message = new StringBuilder();
            message.AppendLine(dto.CreateUpdateDate.ToString());
            message.AppendFormat("Hi {0}!\n", dto.FirstName);
            message.AppendLine("CONTACT INFO REQUEST ACCEPTED");
            message.AppendFormat("You now Share Contact Information with {0}!\n", dto.UserName);
            message.AppendLine("Go and check it out!");
            message.AppendFormat("http://127.0.0.2:81/User/{0}\n", dto.UserID);
            message.AppendLine("Thanks");
            message.AppendLine("http://127.0.0.2:81/");
            return new MailMessage(
            "notification@hyperlocal.dk", // From
            dto.Email, // To
            String.Format("You now Share Contact Information with {0}!", dto.UserName), // Subject
            message.ToString() // Body
            );
        }


        private MailMessage MakeNewsEmailMessage(NotificationDto dto)
        {
            var message = new StringBuilder();
            message.AppendLine(dto.CreateUpdateDate.ToString());
            message.AppendFormat("Hi {0}!\n", dto.FirstName);
            message.AppendLine("NEWS");
            message.AppendLine(dto.Title);
            message.AppendFormat("From community \"{0}\" in category \"{1}\"\n", dto.CommunityName, dto.CategoryName);
            if (dto.AssignmentTitle != null)
            {
                message.AppendFormat("On assignment \"{0}\"\n", dto.AssignmentTitle);
            }
            message.AppendFormat("Posted by {0}\n", dto.PostedByUserName);
            message.AppendLine("Go and check it out!");
            message.AppendFormat("http://127.0.0.2:81/News/{0}\n", dto.NewsItemID);
            message.AppendLine("Thanks");
            message.AppendLine("http://127.0.0.2:81/");
            return new MailMessage(
            "notification@hyperlocal.dk", // From
            dto.Email, // To
            dto.Title, // Subject
            message.ToString() // Body
            );
        }


        private MailMessage MakeMessageReceivedEmailMessage(NotificationDto dto)
        {
            var message = new StringBuilder();
            message.AppendLine(dto.CreateUpdateDate.ToString());
            message.AppendFormat("Hi {0}!\n", dto.FirstName);
            message.AppendLine("You have a new message!");
            message.AppendFormat("{0} says: {1}\n", dto.PostedByUserName, Truncate(dto.MessageBody, 20));
            message.AppendLine("Go and check it out!");
            message.AppendLine("http://127.0.0.2:81/Message/Inbox");
            message.AppendLine("Thanks");
            message.AppendLine("http://127.0.0.2:81/");
            return new MailMessage(
            "notification@hyperlocal.dk", // From
            dto.Email, // To
            String.Format("New Message from {0}", dto.PostedByUserName), // Subject
            message.ToString() // Body
            );
        }


        private MailMessage MakeNewCommentEmailMessage(NotificationDto dto)
        {
            var message = new StringBuilder();
            message.AppendLine(dto.CreateUpdateDate.ToString());
            message.AppendFormat("Hi {0}!\n", dto.FirstName);
            message.AppendLine("NEW COMMENT");
            message.AppendFormat("{0} commented on \"{1}\"\n", dto.PostedByUserName, dto.Title);
            message.AppendFormat("{0} wrote \"{1}\"\n", dto.PostedByUserName, dto.CommentBody);
            message.AppendLine("Go and check it out!");
            message.AppendFormat("http://127.0.0.2:81/News/{0}\n", dto.NewsItemID);
            message.AppendLine("Thanks");
            message.AppendLine("http://127.0.0.2:81/");
            return new MailMessage(
            "notification@hyperlocal.dk", // From
            dto.Email, // To
            String.Format("{0} commented on {1}", dto.PostedByUserName, dto.Title), // Subject
            message.ToString() // Body
            );
        }


        private MailMessage MakeNotificationFromEditorEmailMessage(NotificationDto dto)
        {
            var message = new StringBuilder();
            message.AppendLine(dto.CreateUpdateDate.ToString());
            message.AppendFormat("Hi {0}!\n", dto.FirstName);
            message.AppendLine("NEW ASSIGNMENT in the area around:");
            message.AppendFormat(dto.AssignmentCenterAddress + " within a radius of {0} km\n", dto.AssignmentRadius);
            message.AppendLine(dto.AssignmentTitle);
            message.AppendLine(Truncate(dto.Description, 70));
            message.AppendLine("Go and check it out!");
            message.AppendFormat("http://127.0.0.2:81/Assignment/{0}\n", dto.AssignmentID);
            message.AppendLine("Do you know something? Please let us know!");
            message.AppendLine("Thanks");
            message.AppendLine("http://127.0.0.2:81/");
            return new MailMessage(
            "notification@hyperlocal.dk", // From
            dto.Email, // To
            "Assignment: " + dto.AssignmentTitle, // Subject
            message.ToString() // Body
            );
        }
    }
}