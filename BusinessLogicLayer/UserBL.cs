using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using HLServiceRole.DataTransferObjects;
using HLServiceRole.EntityFramework;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        public UserDto GetUserBL(int userId)
        {
            try
            {
                var userDto = new UserDto();

                var user = entityFramework.procGetUser(userId).SingleOrDefault();

                var userPhoto = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == userId).SingleOrDefault();

                /* We are adding the different sizes of the user photo blob URIs to the UserDto. */
                if (userPhoto != null)
                {
                    userDto.ImageBlobUri = GetSasUriForBlobReadBL(userPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                    userDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(userPhoto.MediumSizeBlobUri);
                    userDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(userPhoto.ThumbnailBlobUri);
                }

                if (user != null)
                {
                    userDto.UserID = user.UserID;
                    userDto.Email = user.Email;
                    userDto.Password = user.Password;
                    userDto.FirstName = user.FirstName;
                    userDto.LastName = user.LastName;
                    userDto.FullName = user.FirstName + " " + user.LastName;
                    userDto.Bio = user.Bio;
                    userDto.PhoneNumber = user.PhoneNumber;
                    userDto.Address = user.Address;
                    userDto.AddressPositionPointWkt = user.AddressPositionPointWkt;
                    userDto.CreateDate = user.CreateDate;
                    userDto.ProfileLastUpdatedDate = user.ProfileLastUpdatedDate;
                    userDto.Blocked = user.Blocked;
                    userDto.PushNotificationChannel = user.PushNotificationChannel;
                    userDto.LastLoginPositionPointWkt = user.LastLoginPositionPointWkt;
                    userDto.LastLoginDateTime = user.LastLoginDateTime;
                    userDto.Latitude = Convert.ToDouble(ExtractLatitudeFromPointWkt(user.AddressPositionPointWkt));
                    userDto.Longitude = Convert.ToDouble(ExtractLongitudeFromPointWkt(user.AddressPositionPointWkt));
                    userDto.LastLoginLatitude = Convert.ToDouble(ExtractLatitudeFromPointWkt(user.LastLoginPositionPointWkt));
                    userDto.LastLoginLongitude = Convert.ToDouble(ExtractLongitudeFromPointWkt(user.LastLoginPositionPointWkt));
                    userDto.HasPhoto = DoesUserHavePhoto(user.UserID);

                    return userDto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.GetUserBL(): " + ex.ToString());
                return null;
            }
        }


        public int CreateUserBL(UserDto userDto)
        {
            try
            {
                var userId = entityFramework.procCreateUser
                (
                    userDto.Email,
                    userDto.Password,
                    userDto.FirstName,
                    userDto.LastName,
                    userDto.Bio,
                    userDto.PhoneNumber,
                    userDto.Address,
                    ConvertLatLongToPointWkt(userDto.Longitude, userDto.Latitude),
                    DateTime.Now,
                    DateTime.Now,
                    userDto.RoleID,
                    userDto.Blocked,
                    string.Empty,
                    ConvertLatLongToPointWkt(userDto.LastLoginLongitude, userDto.LastLoginLatitude),
                    DateTime.Now
                ).Single();

                /* This hooks the new User up with the notification (alert) system, this system sends out notifications on
                 * on news, contact requests, message received a.s.o. */
                entityFramework.procCreateUserAlerts(Convert.ToInt32(userId));

                return Convert.ToInt32(userId);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.CreateUserBL(): " + ex.ToString());
                return -1;
            }
        }


        public bool UpdateUserBL(UserDto userDto)
        {
            try
            {
                entityFramework.procUpdateUser
                (
                    userDto.UserID,
                    userDto.Email,
                    userDto.FirstName,
                    userDto.LastName,
                    userDto.Bio,
                    userDto.PhoneNumber,
                    userDto.Address,
                    ConvertLatLongToPointWkt(userDto.Longitude, userDto.Latitude),
                    DateTime.Now
                );
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.UpdateUserBL(): " + ex.ToString());
                return false;
            }
        }


        public bool DeleteUserBL(int userId)
        {
            try
            {
                /* Deleting a Users Message from Azure table storage. */
                DeleteUsersInbox(userId);
                DeleteUsersOutbox(userId);

                /* Deleting the Comments posted by the User from Azure table storage.*/
                DeleteCommentsPostedByUser(userId);

                /* Deleting NewsItems created by User. */
                var newsItemIds = entityFramework.procGetNewsItemIdsOfNewsItemsCreatedByUser(userId);
                foreach (var n in newsItemIds)
                    DeleteNewsItemBL(Convert.ToInt32(n));

                /* Deleting the Polls created by User. */
                var pollIds = entityFramework.procGetPollIdsOfPollsCreatedByUser(userId);
                foreach (var p in pollIds)
                    DeleteNewsItemBL(Convert.ToInt32(p));

                /* Deleting Communities created by User. */
                var communityIds = entityFramework.procGetCommunityIdsOfCommunitiesCreatedByUser(userId);
                foreach (var c in communityIds)
                    DeleteCommunityBL(Convert.ToInt32(c));

                /* Deleting Assignments created by User. */
                var assignmentIds = entityFramework.procGetAssignmentIdsOfAssignmentsCreatedByUser(userId);
                foreach (var a in assignmentIds)
                    DeleteAssignmentBL(Convert.ToInt32(a));

                /* Deleting UserPhoto */
                var photo = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == userId).SingleOrDefault();
                if (photo != null)
                {
                    DeleteMediaBL(photo.MediaID, MediaUsage.User);
                }

                /* Before this SPROC deletes the User it also
                - Deletes those rows from PollUsers, which have been used to register that a particular User has voted in a CommunityPoll.
                - Deletes those rows from UserRoles where a User is assigned to a Role.
                - Deletes ContactInfoRequests which involves the User.
                - Deletes from ContactInfoUsers those rows which indicates which other Users the User shares contact information with.
                - Deletes the Users UserAlert settings.
                - Deletes those UserFollowCommunities rows where the User follows a Community. */
                entityFramework.procDeleteUser(userId);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.DeleteUserBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Returns a Users UserID in case their credentials are valid. Otherwise returns -1.
        /// </summary>
        public int UserLoginBL(string email, string password)
        {
            try
            {
                var userId = (int)Convert.ToInt32(entityFramework.procLogin(email, password).Single());

                if (userId > 0)
                    return userId;
                else
                    return -1;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.UserLoginBL(): " + ex.ToString());
                return -1;
            }
        }


        /// <summary>
        /// Checks whether a user has been blocked
        /// </summary>
        public bool IsUserBlockedBL(string email, string password)
        {
            try
            {
                var isUserBlocked = entityFramework.procIsUserBlocked(email, password).Single();

                if (isUserBlocked > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.IsUserBlockedBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Checks whether an email is already in use.
        /// </summary>
        public bool IsEmailInUseBL(string email)
        {
            try
            {
                var isEmailInUse = entityFramework.procIsEmailInUse(email).Single();

                if (isEmailInUse > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.IsEmailInUseBL(): " + ex.ToString());
                return false;
            }
        }


        public bool ChangePasswordBL(string email, string oldPassword, string newPassword)
        {
            try
            {
                var userId = (int)Convert.ToInt32(entityFramework.procChangePassword(email, oldPassword, newPassword).Single());

                if (userId > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.ChangePasswordBL: " + ex.ToString());
                return false;
            }
        }


        public string GetUserEmailBL(int userId)
        {
            try
            {
                var user = GetUserBL(userId);

                if (user != null)
                {
                    return user.Email;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.GetUserEmailBL(): " + ex.ToString());
                return null;
            }
        }


        public string[] GetRolesForUserBL(string userEmail)
        {
            try
            {
                var roles = entityFramework.procGetRolesForUser(userEmail);

                if (roles != null)
                {
                    var roleList = roles.ToList();
                    var numberOfRoles = roleList.Count();
                    var roleArray = new string[numberOfRoles];
                    var index = 0;

                    foreach (var r in roleList)
                    {
                        roleArray[index] = r.RoleName;
                        index++;
                    }
                    return roleArray;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.GetRolesForUserBL: " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Updates the Users table with the time and position of a Users latests login.
        /// </summary>
        public bool UpdateLastLoginPositionBL(int userId, double latitude, double longitude)
        {
            try
            {
                entityFramework.procUpdateLastLoginPosition(userId, ConvertLatLongToPointWkt(longitude, latitude));
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.UpdateLastLoginPositionBL(): " + ex.ToString());
                return false;
            }
        }


        public List<UserDto> GetLatestActiveUsersFromDkCountryBL(int pageSize, int pageNumber)
        {
            try
            {
                var users = entityFramework.procGetLatestActiveUsersFromDkCountry(pageSize, pageNumber);

                var userDtos = new List<UserDto>();

                if (users != null)
                {
                    foreach (var u in users)
                    {
                        var userDto = new UserDto()
                        {
                            UserID = u.UserID,
                            FullName = u.FirstName + " " + u.LastName,
                            AddressPositionPointWkt = u.AddressPositionPointWkt,
                            LastLoginPositionPointWkt = u.LastLoginPositionPointWkt,
                            LastLoginDateTime = u.LastLoginDateTime,
                            LatestActivity = u.LatestActivity,
                            NumberOfNewsItemsPostedByUser = GetNumberOfNewsItemsPostedByUser(u.UserID),
                            HasPhoto = DoesUserHavePhoto(u.UserID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(u.HasNextPageOfData)
                        };

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like data type DateTime. */
                        if (u.LatestActivity != null)
                            userDto.LatestActivityToString = "Latests Activity: " + u.LatestActivity.ToString();
                        else
                            userDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var userPhoto = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == u.UserID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (userPhoto != null)
                        {
                            userDto.ImageBlobUri = GetSasUriForBlobReadBL(userPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            userDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(userPhoto.MediumSizeBlobUri);
                            userDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(userPhoto.ThumbnailBlobUri);
                        }
                        userDtos.Add(userDto);
                    }
                    return userDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.GetLatestActiveUsersFromDkCountry(): " + ex.ToString());
                return null;
            }
        }


        public List<UserDto> GetLatestActiveUsersFromRegionBL(string urlRegionName, int pageSize, int pageNumber)
        {
            try
            {
                var users = entityFramework.procGetLatestActiveUsersFromRegion(urlRegionName, pageSize, pageNumber);

                var userDtos = new List<UserDto>();

                if (users != null)
                {
                    foreach (var u in users)
                    {
                        var userDto = new UserDto()
                        {
                            UserID = u.UserID,
                            FullName = u.FirstName + " " + u.LastName,
                            AddressPositionPointWkt = u.AddressPositionPointWkt,
                            LastLoginPositionPointWkt = u.LastLoginPositionPointWkt,
                            LastLoginDateTime = u.LastLoginDateTime,
                            LatestActivity = u.LatestActivity,
                            NumberOfNewsItemsPostedByUser = GetNumberOfNewsItemsPostedByUser(u.UserID),
                            HasPhoto = DoesUserHavePhoto(u.UserID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(u.HasNextPageOfData)
                        };

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like data type DateTime. */
                        if (u.LatestActivity != null)
                            userDto.LatestActivityToString = "Latests Activity: " + u.LatestActivity.ToString();
                        else
                            userDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var userPhoto = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == u.UserID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (userPhoto != null)
                        {
                            userDto.ImageBlobUri = GetSasUriForBlobReadBL(userPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            userDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(userPhoto.MediumSizeBlobUri);
                            userDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(userPhoto.ThumbnailBlobUri);
                        }
                        userDtos.Add(userDto);
                    }
                    return userDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.GetLatestActiveUsersFromRegionBL(): " + ex.ToString());
                return null;
            }
        }


        public List<UserDto> GetLatestActiveUsersFromMunicipalityBL(string urlMunicipalityName, int pageSize, int pageNumber)
        {
            try
            {
                var users = entityFramework.procGetLatestActiveUsersFromMunicipality(urlMunicipalityName, pageSize, pageNumber);

                var userDtos = new List<UserDto>();

                if (users != null)
                {
                    foreach (var u in users)
                    {
                        var userDto = new UserDto()
                        {
                            UserID = u.UserID,
                            FullName = u.FirstName + " " + u.LastName,
                            AddressPositionPointWkt = u.AddressPositionPointWkt,
                            LastLoginPositionPointWkt = u.LastLoginPositionPointWkt,
                            LastLoginDateTime = u.LastLoginDateTime,
                            LatestActivity = u.LatestActivity,
                            NumberOfNewsItemsPostedByUser = GetNumberOfNewsItemsPostedByUser(u.UserID),
                            HasPhoto = DoesUserHavePhoto(u.UserID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(u.HasNextPageOfData)
                        };

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like data type DateTime. */
                        if (u.LatestActivity != null)
                            userDto.LatestActivityToString = "Latests Activity: " + u.LatestActivity.ToString();
                        else
                            userDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var userPhoto = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == u.UserID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (userPhoto != null)
                        {
                            userDto.ImageBlobUri = GetSasUriForBlobReadBL(userPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            userDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(userPhoto.MediumSizeBlobUri);
                            userDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(userPhoto.ThumbnailBlobUri);
                        }
                        userDtos.Add(userDto);
                    }
                    return userDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.GetLatestActiveUsersFromMunicipalityBL(): " + ex.ToString());
                return null;
            }
        }


        public List<UserDto> GetLatestActiveUsersFromPostalCodeBL(string POSTNR_TXT, int pageSize, int pageNumber)
        {
            try
            {
                var users = entityFramework.procGetLatestActiveUsersFromPostalCode(POSTNR_TXT, pageSize, pageNumber);

                var userDtos = new List<UserDto>();

                if (users != null)
                {
                    foreach (var u in users)
                    {
                        var userDto = new UserDto()
                        {
                            UserID = u.UserID,
                            FullName = u.FirstName + " " + u.LastName,
                            AddressPositionPointWkt = u.AddressPositionPointWkt,
                            LastLoginPositionPointWkt = u.LastLoginPositionPointWkt,
                            LastLoginDateTime = u.LastLoginDateTime,
                            LatestActivity = u.LatestActivity,
                            NumberOfNewsItemsPostedByUser = GetNumberOfNewsItemsPostedByUser(u.UserID),
                            HasPhoto = DoesUserHavePhoto(u.UserID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(u.HasNextPageOfData)
                        };

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like data type DateTime. */
                        if (u.LatestActivity != null)
                            userDto.LatestActivityToString = "Latests Activity: " + u.LatestActivity.ToString();
                        else
                            userDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var userPhoto = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == u.UserID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (userPhoto != null)
                        {
                            userDto.ImageBlobUri = GetSasUriForBlobReadBL(userPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            userDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(userPhoto.MediumSizeBlobUri);
                            userDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(userPhoto.ThumbnailBlobUri);
                        }
                        userDtos.Add(userDto);
                    }
                    return userDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.GetLatestActiveUsersFromPostalCodeBL(): " + ex.ToString());
                return null;
            }
        }


        public List<UserDto> GetAllUsersBL()
        {
            try
            {
                var users = entityFramework.procGetAllUsers();

                var userDtos = new List<UserDto>();

                if (users != null)
                {
                    foreach (var u in users)
                    {
                        var userDto = new UserDto()
                        {
                            UserID = u.UserID,
                            FullName = u.FullName
                        };

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var userPhoto = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == u.UserID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (userPhoto != null)
                        {
                            userDto.ImageBlobUri = GetSasUriForBlobReadBL(userPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            //userDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(userPhoto.MediumSizeBlobUri);
                            userDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(userPhoto.ThumbnailBlobUri);
                        }
                        userDtos.Add(userDto);
                    }
                    return userDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.GetAllUsersBL(): " + ex.ToString());
                return null;
            }
        }


        public int GetNumberOfRequestsAndUnreadMessagesToUserBL(int userId)
        {
            try
            {
                var number = GetNumberOfContactInfoRequestsToUserBL(userId);
                number += GetNumberOfUnreadMessagesBL(userId);
                return number;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in UserBL.GetNumberOfRequestsAndUnreadMessagesToUserBL(): " + ex.ToString());
                return 0;
            }
        }
    }
}