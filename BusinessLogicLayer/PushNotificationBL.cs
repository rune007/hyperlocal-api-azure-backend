using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.AzureTableStorage;
using HLServiceRole.DataTransferObjects;
using System.Text;
using System.IO;
using System.Net;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        /// <summary>
        /// Switch for sending out various push notifications.
        /// </summary>
        public void NotifyUserByPushNotification(NotificationDto dto, NotificationKind notificationKind)
        {
            switch (notificationKind)
            {
                case NotificationKind.BreakingNews:
                    SendBreakingNewsPushNotification(dto);
                    break;
                case NotificationKind.News:
                    SendNewsPushNotification(dto);
                    break;
                case NotificationKind.ContactRequest:
                    SendContactRequestPushNotification(dto);
                    break;
                case NotificationKind.ContactRequestAccepted:
                    SendContactRequestAcceptedPushNotification(dto);
                    break;
                case NotificationKind.MessageReceived:
                    SendMessageReceivedPushNotification(dto);
                    break;
                case NotificationKind.Comment:
                    SendCommentPushNotification(dto);
                    break;
                case NotificationKind.NotificationFromEditors:
                    SendNotificationFromEditorsPushNotification(dto);
                    break;
            }
        }


        /// <summary>
        /// As the space is sparse for a toast push notification we use the following abbreviations:
        /// BRK - Breaking News
        /// NEW - News
        /// CON - ContactInfoRequest
        /// ACC - Accept of ContactInfoRequest
        /// MSG - Message
        /// COM - New Comment
        /// ASS - New Assignment
        /// </summary>
        private void SendBreakingNewsPushNotification(NotificationDto dto)
        {
            SendPushNotificationToClientBL(dto.PushNotificationChannel, "HLApp:", "BRK: " + dto.Title);
        }


        private void SendNewsPushNotification(NotificationDto dto)
        {
            SendPushNotificationToClientBL(dto.PushNotificationChannel, "HLApp:", "NEW: " + dto.Title);
        }


        private void SendContactRequestPushNotification(NotificationDto dto)
        {
            SendPushNotificationToClientBL(dto.PushNotificationChannel, "HLApp:", "CON: " + dto.PostedByUserName);
        }


        private void SendContactRequestAcceptedPushNotification(NotificationDto dto)
        {
            SendPushNotificationToClientBL(dto.PushNotificationChannel, "HLApp:", "ACC: " + dto.UserName);
        }


        private void SendMessageReceivedPushNotification(NotificationDto dto)
        {
            SendPushNotificationToClientBL(dto.PushNotificationChannel, "HLApp:", "MSG: " + dto.PostedByUserName);
        }


        private void SendCommentPushNotification(NotificationDto dto)
        {
            SendPushNotificationToClientBL(dto.PushNotificationChannel, "HLApp:", "COM: " + dto.Title);
        }


        private void SendNotificationFromEditorsPushNotification(NotificationDto dto)
        {
            SendPushNotificationToClientBL(dto.PushNotificationChannel, "HLApp:", "ASS: " + dto.AssignmentCenterAddress);
        }


        /// <summary>
        /// Register Push Notification Channel URI coming in from a particular application in a particular Windows Phone 7 device.
        /// </summary>
        public bool RegisterPushNotificationChannelBL(int userId, string channel)
        {
            try
            {
                entityFramework.procUpdatePushNotificationChannel(userId, channel);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in PushNotificationBL.RegisterPushNotificationChannelBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Sends off an actual toast push notification to the Windows Phone client.
        /// My implementation of Push Notification is based upon code I found in a book and have modified to my purpose for HLApp. The code example 
        /// I am using was found in the book Lee/Chuvyrov, "Beginning Windows Phone 7 Development", Apress, USA 2010, p. 353 - 366.
        /// </summary>
        public void SendPushNotificationToClientBL(string url, string notificationTitle, string notificationText)
        {
            /* Template for a toast push notification message. */
            string ToastPushXML = "<?xml version=\"1.0\" encoding=\"utf-8\"?>" +
                            "<wp:Notification xmlns:wp=\"WPNotification\">" +
                                "<wp:Toast>" +
                                    "<wp:Text1>{0}</wp:Text1>" +
                                    "<wp:Text2>{1}</wp:Text2>" +
                                "</wp:Toast>" +
                            "</wp:Notification>";

            HttpWebRequest sendNotificationRequest = (HttpWebRequest)WebRequest.Create(url);

            sendNotificationRequest.Method = "POST";
            sendNotificationRequest.Headers = new WebHeaderCollection();
            sendNotificationRequest.ContentType = "text/xml";

            sendNotificationRequest.Headers.Add("X-WindowsPhone-Target", "toast");
            sendNotificationRequest.Headers.Add("X-NotificationClass", "2");
            string str = string.Format(ToastPushXML, notificationTitle, notificationText);

            byte[] strBytes = new UTF8Encoding().GetBytes(str);
            sendNotificationRequest.ContentLength = strBytes.Length;
            using (Stream requestStream = sendNotificationRequest.GetRequestStream())
            {
                requestStream.Write(strBytes, 0, strBytes.Length);
            }

            try
            {
                HttpWebResponse response = (HttpWebResponse)sendNotificationRequest.GetResponse();
                string notificationStatus = response.Headers["X-NotificationStatus"];           //(Received|Dropped|QueueFull|)
                string deviceConnectionStatus = response.Headers["X-DeviceConnectionStatus"];   //(Connected|InActive|Disconnected|TempDisconnected)
            }
            catch (Exception ex)
            {
                var exp = ex;
                Trace.TraceError("Problem in PushNotificationBL.SendPushNotificationToClientBL(): " + ex.ToString());
            }           
        }
    }
}