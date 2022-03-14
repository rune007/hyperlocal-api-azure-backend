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
        public bool AreUsersSharingContactInfoBL(int userAId, int userBId)
        {
            try
            {
                var status = entityFramework.procAreUsersSharingContactInfo(userAId, userBId).Single();

                if (status > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in ContactInfoBL.AreUsersSharingContactInfoBL(): " + ex.ToString());
                return false;
            }
        }


        public bool IsContactInfoRequestPendingBL(int userAId, int userBId)
        {
            try
            {
                var status = entityFramework.procIsContactInfoRequestPending(userAId, userBId).Single();

                if (status > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in ContactInfoBL.IsContactInfoRequestPendingBL(): " + ex.ToString());
                return false;
            }
        }


        public bool RequestContactInformationBL(int fromUserId, int toUserId)
        {
            try
            {
                var contactInfoRequest = entityFramework.procRequestContactInformation(fromUserId, toUserId).SingleOrDefault();

                if (contactInfoRequest != null)
                {
                    var dto = new ContactInfoRequestDto()
                    {
                        ContactInfoRequestID = contactInfoRequest.ContactInfoRequestID,
                        FromUserID = contactInfoRequest.FromUserID,
                        ToUserID = contactInfoRequest.ToUserID,
                        CreateDate = contactInfoRequest.CreateDate
                    };

                    /* Alerts the receiving User about the ContactInfoRequest. */
                    AlertUserOnContactInfoRequest(dto);
                    return true;
                }
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in ContactInfoBL.RequestContactInformationBL(): " + ex.ToString());
                return false;
            }
        }


        public List<ContactInfoRequestDto> GetContactInfoRequestsToUserBL(int userId, int pageSize, int pageNumber)
        {
            try
            {
                var requests = entityFramework.procGetContactInfoRequestsToUser(userId, pageSize, pageNumber);

                var requestDtos = new List<ContactInfoRequestDto>();

                if (requests != null)
                {
                    foreach (var r in requests)
                    {
                        var dto = new ContactInfoRequestDto()
                        {
                            ContactInfoRequestID = r.ContactInfoRequestID,
                            FromUserID = r.FromUserID,
                            FromUserName = r.FromUserName,
                            FromUserPhotoUri = GetUserPhotoMedium(r.FromUserID),
                            ToUserID = r.ToUserID,
                            CreateDate = r.CreateDate,
                            NumberOfRequests = Convert.ToInt32(r.NumberOfRequests),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(r.HasNextPageOfData)
                        };
                        requestDtos.Add(dto);
                    }
                    return requestDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in Problem in ContactInfoBL.GetContactInfoRequestsToUserBL(): " + ex.ToString());
                return null;
            }
        }


        public int GetNumberOfContactInfoRequestsToUserBL(int userId)
        {
            try
            {
                var number = Convert.ToInt32(entityFramework.procGetNumberOfContactInfoRequestsToUser(userId).Single());
                return number;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in ContactInfoBL.GetNumberOfContactInfoRequestsToUserBL(): " + ex.ToString());
                return 0;
            }
        }


        public bool AcceptContactInfoRequestBL(int contactInfoRequestId, int fromUserId, int toUserId)
        {
            try
            {
                entityFramework.procAcceptContactInfoRequest(contactInfoRequestId);
                /* Alerts the User who requested the contact information that their request have been accepted. */
                AlertUserOnContactInfoRequestAccepted(fromUserId, toUserId);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in ContactInfoBL.AcceptContactInfoRequestBL(): " + ex.ToString());
                return false;
            }
        }


        public bool RejectContactInfoRequestBL(int contactInfoRequestId)
        {
            try
            {
                entityFramework.procRejectContactInfoRequest(contactInfoRequestId);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in ContactInfoBL.AcceptContactInfoRequestBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Makes two Users stop sharing contact information.
        /// </summary>
        public bool StopSharingContactInfoBL(int userAId, int userBId)
        {
            try
            {
                entityFramework.procStopSharingContactInfo(userAId, userBId);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in ContactInfoBL.StopSharingContactInfoBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Gets a list of Users who are sharing contact information (Email, PhoneNumber, Address) with a User.
        /// </summary>
        public List<UserDto> GetUsersWhoAreSharingContactInfoWithUserBL(int userId, int pageSize, int pageNumber)
        {
            try
            {
                var users = entityFramework.procGetUsersWhoAreSharingContactInformationWithUser(userId, pageSize, pageNumber);

                var userDtos = new List<UserDto>();

                if (users != null)
                {
                    foreach (var u in users)
                    {
                        var userDto = new UserDto()
                        {
                            UserID = u.UserID,
                            FullName = u.UserFullName,
                            Email = u.Email,
                            PhoneNumber = u.PhoneNumber,
                            Address = u.Address,
                            Latitude = ExtractLatitudeFromPointWkt(u.AddressPositionPointWkt),
                            Longitude = ExtractLongitudeFromPointWkt(u.AddressPositionPointWkt),
                            LastLoginLatitude = ExtractLatitudeFromPointWkt(u.LastLoginPositionPointWkt),
                            LastLoginLongitude = ExtractLongitudeFromPointWkt(u.LastLoginPositionPointWkt),
                            LastLoginDateTime = u.LastLoginDateTime,
                            HasPhoto = DoesUserHavePhoto(u.UserID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(u.HasNextPageOfData),
                        };

                        /* Finding and adding an eventual photo. */
                        var userPhoto = entityFramework.Media.OfType<UserPhoto>().Where(p => p.UserID == u.UserID).SingleOrDefault();
                        /* We are adding the different sizes of the photo blob URIs. */
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
                Trace.TraceError("Problem in ContactInfoBL.GetUsersWhoAreSharingContactInfoWithUserBL(): " + ex.ToString());
                return null;
            }
        }
    }
}