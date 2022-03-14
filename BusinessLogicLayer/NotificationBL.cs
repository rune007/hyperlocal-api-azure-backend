using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.AzureTableStorage;
using HLServiceRole.DataTransferObjects;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        /// <summary>
        /// The different kinds of notifications the Users can receive via email and push notification.
        /// </summary>
        public enum NotificationKind { BreakingNews, News, ContactRequest, ContactRequestAccepted, MessageReceived, Comment, NotificationFromEditors };


        /// <summary>
        /// Gets the alert settings for a particular User. (How to receive notifications on Comments, Messages, news, etc.)
        /// </summary>
        public UserAlertDto GetUserAlertsBL(int userId)
        {
            try
            {
                var userAlertDto = new UserAlertDto();

                var userAlerts = entityFramework.procGetUserAlerts(userId).SingleOrDefault();

                if (userAlerts != null)
                {
                    userAlertDto.UserID = userAlerts.UserID;
                    userAlertDto.AlertOnNews = userAlerts.AlertOnNews;
                    userAlertDto.AlertOnBreakingNews = userAlerts.AlertOnBreakingNews;
                    userAlertDto.AlertOnContactRequests = userAlerts.AlertOnContactRequests;
                    userAlertDto.AlertOnMessages = userAlerts.AlertOnMessages;
                    userAlertDto.AlertOnComments = userAlerts.AlertOnComments;
                    userAlertDto.AlertOnNotificationFromEditors = userAlerts.AlertOnNotificationsFromEditors;
                    userAlertDto.SendEmail = userAlerts.SendEmail;
                    userAlertDto.UsePushNotification = userAlerts.UsePushNotification;

                    return userAlertDto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NotificationBL.GetUserAlertsBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// UserAlerts are the settings which determines which notifications the Users receive from the system and how they receive them.
        /// That is AlertOnBreakingNews, AlertOnMessages, SendEmail, UsePushNotification, etc.
        /// </summary>
        public bool UpdateUserAlertsBL(UserAlertDto dto)
        {
            try
            {
                entityFramework.procUpdateUserAlerts
                (
                    dto.UserID,
                    dto.AlertOnNews,
                    dto.AlertOnBreakingNews,
                    dto.AlertOnContactRequests,
                    dto.AlertOnMessages,
                    dto.AlertOnComments,
                    dto.AlertOnNotificationFromEditors,
                    dto.SendEmail,
                    dto.UsePushNotification
                );
                return true;
            }

            catch (Exception ex)
            {
                Trace.TraceError("Problem in NotificationBL.UpdateUserAlertsBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Alerts the Users who follow a Community wherein the NewsItem is located and who, of course, also wants to be notified about news.
        /// </summary>
        private void AlertUsersOnNewsItemPosted(int newsItemId)
        {
            try
            {
                var newsItemDto = GetNewsItemBL(newsItemId);

                /* If the NewsItem IsLocalBreakingNews, we alert everybody who follows a Community wherein the NewsItem is located. */
                if (newsItemDto.IsLocalBreakingNews)
                {
                    var users = entityFramework.procGetUsersToAlertOnBreakingNews(newsItemDto.NewsItemID, newsItemDto.PostedByUserID);

                    foreach (var u in users)
                    {
                        var dto = new NotificationDto()
                        {
                            UserID = u.UserID,
                            Email = u.Email,
                            FirstName = u.FirstName,
                            PhoneNumber = u.PhoneNumber,
                            PushNotificationChannel = u.PushNotificationChannel,
                            NewsItemID = newsItemDto.NewsItemID,
                            Title = newsItemDto.Title,
                            CreateUpdateDate = newsItemDto.CreateUpdateDate,
                            CategoryName = newsItemDto.CategoryName,
                            AssignmentID = newsItemDto.AssignmentID,
                            AssignmentTitle = newsItemDto.AssignmentTitle,
                            PostedByUserName = newsItemDto.PostedByUserName,
                            CommunityName = u.CommunityName,
                            SendEmail = u.SendEmail,
                            UsePushNotification = u.UsePushNotification
                        };

                        NotifyUser(dto, NotificationKind.BreakingNews);
                    }
                }
                /* If the NewsItem is not IsLocalBreakingNews we only notify those who wants alerts on all news from that Community. */
                else
                {
                    var users = entityFramework.procGetUsersToAlertOnNews(newsItemDto.NewsItemID, newsItemDto.PostedByUserID);

                    foreach (var u in users)
                    {
                        var dto = new NotificationDto()
                        {
                            UserID = u.UserID,
                            Email = u.Email,
                            FirstName = u.FirstName,
                            PhoneNumber = u.PhoneNumber,
                            PushNotificationChannel = u.PushNotificationChannel,
                            NewsItemID = newsItemDto.NewsItemID,
                            Title = newsItemDto.Title,
                            CreateUpdateDate = newsItemDto.CreateUpdateDate,
                            CategoryName = newsItemDto.CategoryName,
                            AssignmentID = newsItemDto.AssignmentID,
                            AssignmentTitle = newsItemDto.AssignmentTitle,
                            PostedByUserName = newsItemDto.PostedByUserName,
                            CommunityName = u.CommunityName,
                            SendEmail = u.SendEmail,
                            UsePushNotification = u.UsePushNotification
                        };

                        NotifyUser(dto, NotificationKind.News);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NotificationBL.AlertUsersOnNewsItemPosted(): " + ex.ToString());
            }
        }


        /// <summary>
        /// Because I have chosen to store Comments on Azure table storage, and the information about NewsItems and UserAlerts on SQL Azure (SQL Server),
        /// this method is slightly complex. If all the data had been on SQL Azure I could just write a single query joining the various data.
        /// But as it is now I need to go and get it the various places (Azure table storage and SQL Azure) and then join.
        /// </summary>
        private void AlertUsersOnCommentPosted(CommentDto commentDto)
        {
            try
            {
                // Getting the Title of the NewsItem being commented
                var newsItemTitle = GetNewsItemBL(commentDto.NewsItemID).Title;

                // List of Users who might want an alert on the new Comment, we don't know yet because the data about who want what alert is stored
                // on SQL Azure and the information about Comments is stored in Azure table storage.
                var usersWhoMightWantAlertOnComment = new List<int>();

                // Getting a list of Users who also commented the NewsItem from Azure table storage. Excluded on the list is naturally the User who posted the Comment.
                usersWhoMightWantAlertOnComment = azureTableDataSource.GetUsersWhoAlsoCommentedOnNewsItem(commentDto.NewsItemID, commentDto.PostedByUserID);

                // Getting the User who orginally posted the NewsItem from SQL Azure (SQL Server). We naturally don't get anything if the User posting the Comment
                // is the same as the User who originally posted the NewsItem.
                var userId = entityFramework.procGetUserWhoPostedNewsItemIfNotSameAsPostedComment(commentDto.NewsItemID, commentDto.PostedByUserID).SingleOrDefault();

                if (userId != null)
                {
                    var userIdInt = Convert.ToInt32(userId);
                    usersWhoMightWantAlertOnComment.Add(userIdInt);
                }

                // Removing duplicates.
                usersWhoMightWantAlertOnComment = usersWhoMightWantAlertOnComment.Distinct().ToList();

                // At this point we take our list of users who might want an alert on the comment: usersWhoMightWantAlertOnComment.
                // We then check this list against our UserAlert information on SQL Azure.
                // Only in case a User on the list have their AlertOnComments field set to true will they be included in the 
                // final notificationDtos list. It is this list we use to generate the actual notifications.
                foreach (var u in usersWhoMightWantAlertOnComment)
                {
                    var user = entityFramework.procGetUserToAlertOnComment(u).Single();

                    if (user != null)
                    {
                        var notificationDto = new NotificationDto()
                        {
                            UserID = user.UserID,
                            Email = user.Email,
                            FirstName = user.FirstName,
                            PhoneNumber = user.PhoneNumber,
                            PushNotificationChannel = user.PushNotificationChannel,
                            NewsItemID = commentDto.NewsItemID,
                            Title = newsItemTitle,
                            CommentBody = commentDto.CommentBody,
                            CreateUpdateDate = commentDto.CreateDate,
                            PostedByUserName = commentDto.PostedByUserName,
                            SendEmail = user.SendEmail,
                            UsePushNotification = user.UsePushNotification
                        };

                        NotifyUser(notificationDto, NotificationKind.Comment);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NotificationBL.AlertUsersOnCommentPosted(): " + ex.ToString());
            }
        }


        private void AlertUserOnMessageReceived(int receiverUserId, int senderUserId, string messageBody)
        {
            try
            {
                var user = entityFramework.procGetUserToAlertOnMessageReceived(receiverUserId, senderUserId).SingleOrDefault();

                if (user != null)
                {
                    var notificationDto = new NotificationDto()
                    {
                        Email = user.Email,
                        FirstName = user.FirstName,
                        PushNotificationChannel = user.PushNotificationChannel,
                        MessageBody = messageBody,
                        CreateUpdateDate = DateTime.Now,
                        PostedByUserName = user.PostedByUserName,
                        SendEmail = user.SendEmail,
                        UsePushNotification = user.UsePushNotification
                    };

                    NotifyUser(notificationDto, NotificationKind.MessageReceived);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NotificationBL.AlertUserOnMessageReceived(): " + ex.ToString());
            }
        }


        private void AlertUsersOnGeoTemporalAssignment(AssignmentDto assignmentDto)
        {
            try
            {
                var users = entityFramework.procGetUsersToAlertOnGeoTemporalAssignment(assignmentDto.AssignmentID, assignmentDto.HoursToGoBack);

                if (users != null)
                {
                    foreach (var u in users)
                    {
                        var dto = new NotificationDto()
                        {
                            UserID = u.UserID,
                            Email = u.Email,
                            FirstName = u.FirstName,
                            PhoneNumber = u.PhoneNumber,
                            PushNotificationChannel = u.PushNotificationChannel,
                            SendEmail = u.SendEmail,
                            UsePushNotification = u.UsePushNotification,
                            AssignmentCenterAddress = assignmentDto.AssignmentCenterAddress,
                            AssignmentRadius = assignmentDto.AssignmentRadius,
                            AssignmentID = assignmentDto.AssignmentID,
                            AssignmentTitle = assignmentDto.Title,
                            Description = assignmentDto.Description,
                            CreateUpdateDate = assignmentDto.CreateUpdateDate
                        };

                        NotifyUser(dto, NotificationKind.NotificationFromEditors);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NotificationBL.AlertUsersOnGeoTemporalAssignment(): " + ex.ToString());
            }
        }


        private void AlertUserOnContactInfoRequest(ContactInfoRequestDto dto)
        {
            try
            {
                var user = entityFramework.procGetUserToAlertOnContactInfoRequest(dto.FromUserID, dto.ToUserID).SingleOrDefault();

                if (user != null)
                {
                    var notificationDto = new NotificationDto()
                    {                      
                        FirstName = user.FirstName,
                        Email = user.Email,
                        PushNotificationChannel = user.PushNotificationChannel,
                        SendEmail = user.SendEmail,
                        UsePushNotification = user.UsePushNotification,
                        CreateUpdateDate = dto.CreateDate,
                        PostedByUserName = user.FromUserName
                    };

                    NotifyUser(notificationDto, NotificationKind.ContactRequest);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NotificationBL.AlertUserOnContactInfoRequest(): " + ex.ToString());
            }
        }


        /// <summary>
        /// My implementation of a "Friend" like feature is that Users can share contact information (phone number, email, etc.)
        /// This alert notifies a User that another User have accepted their ShareContactInfoRequest.
        /// </summary>
        /// <param name="fromUserId"></param>
        /// <param name="toUserId"></param>
        private void AlertUserOnContactInfoRequestAccepted(int fromUserId, int toUserId)
        {
            try
            {
                var user = entityFramework.procGetUserToAlertOnContactInfoRequestAccepted(fromUserId, toUserId).SingleOrDefault();

                if (user != null)
                {
                    var notificationDto = new NotificationDto()
                    {
                        FirstName = user.FirstName,
                        Email = user.Email,
                        PushNotificationChannel = user.PushNotificationChannel,
                        SendEmail = user.SendEmail,
                        UsePushNotification = user.UsePushNotification,
                        CreateUpdateDate = Convert.ToDateTime(user.CreateDate),
                        UserID = Convert.ToInt32(user.AcceptingUserID),
                        UserName = user.AcceptingUserName
                    };

                    NotifyUser(notificationDto, NotificationKind.ContactRequestAccepted);
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NotificationBL.AlertUserOnContactInfoRequest(): " + ex.ToString());
            }
        }


        /// <summary>
        /// This method handles which medium to use for the notification, it can be either push notification or email, or both.
        /// </summary>
        private void NotifyUser(NotificationDto dto, NotificationKind notificationKind)
        {
            try
            {
                if (dto.SendEmail == true)
                    NotifyUserByEmail(dto, notificationKind);

                if (dto.UsePushNotification == true)
                    NotifyUserByPushNotification(dto, notificationKind);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NotificationBL.NotifyUser(): " + ex.ToString());
            }
        }
    }
}