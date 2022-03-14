using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Web;
using HLServiceRole.DataTransferObjects;
using HLServiceRole.EntityFramework;
using Microsoft.WindowsAzure.Diagnostics;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        public CommunityDto GetCommunityBL(int communityId)
        {
            try
            {
                var communityDto = new CommunityDto();

                var community = entityFramework.procGetCommunity(communityId).SingleOrDefault();

                var communityPhoto = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == communityId).SingleOrDefault();

                /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                if (communityPhoto != null)
                {
                    communityDto.ImageBlobUri = GetSasUriForBlobReadBL(communityPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                    communityDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(communityPhoto.MediumSizeBlobUri);
                    communityDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(communityPhoto.ThumbnailBlobUri);
                    communityDto.HasPhoto = true;
                }

                if (community != null)
                {
                    communityDto.CommunityID = community.CommunityID;
                    communityDto.AddedByUserID = community.AddedByUserID;
                    communityDto.AddedByUserFullName = community.AddedByUserFirstName + " " + community.AddedByUserLastName;
                    communityDto.Name = community.Name;
                    communityDto.Description = community.Description;
                    communityDto.PolygonWkt = community.PolygonWkt;
                    communityDto.CreateUpdateDate = community.CreateUpdateDate;
                    communityDto.PolygonCenterLatitude = ExtractLatitudeFromPointWkt(community.CenterPointWkt);
                    communityDto.PolygonCenterLongitude = ExtractLongitudeFromPointWkt(community.CenterPointWkt);
                    communityDto.NumberOfUsersInCommunity = Convert.ToInt32(community.NumberOfUsers);
                    communityDto.LatestActivity = community.LatestActivity;
                    /* Adding a string version of LatestActivity. */
                    if (community.LatestActivity != null)
                        communityDto.LatestActivityToString = "Latest Activity: " + community.LatestActivity.ToString();
                    else
                        communityDto.LatestActivityToString = null;

                    return communityDto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.GetCommunityBL(): " + ex.ToString());
                return null;
            }
        }


        public int CreateCommunityBL(CommunityDto communityDto)
        {
            try
            {
                var communityId = entityFramework.procCreateCommunity
                (
                    communityDto.AddedByUserID,
                    communityDto.Name,
                    communityDto.Description,
                    communityDto.PolygonWkt,
                    DateTime.Now
                ).Single();

                if (communityId > 0)
                {
                    /* As default the creator of a Community also becomes a follower of that Community. */
                    UserFollowCommunityBL(communityDto.AddedByUserID, Convert.ToInt32(communityId));
                    return Convert.ToInt32(communityId);
                }
                else
                    return 0;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.CreateCommunityBL(): " + ex.ToString());
                return 0;
            }
        }


        public bool UpdateCommunityBL(CommunityDto communityDto)
        {
            try
            {
                entityFramework.procUpdateCommunity
                (
                    communityDto.CommunityID,
                    communityDto.Name,
                    communityDto.Description,
                    communityDto.PolygonWkt,
                    DateTime.Now
                );
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.UpdateCommunityBL(): " + ex.ToString());
                return false;
            }
        }


        public bool DeleteCommunityBL(int communityId)
        {
            try
            {
                /* Deleting CommunityPhoto */
                var photo = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == communityId).SingleOrDefault();
                if (photo != null)
                {
                    DeleteMediaBL(photo.MediaID, MediaUsage.Community);
                }

                /* Deleting the Polls associated with the Community. */
                DeletePollsAssociatedWithCommunity(communityId);

                /* Deleting the Community, besides deleting the Community this SPROC also deletes those 
                 * UserFollowCommunities rows where a User follows the Community about to be deleted.*/
                entityFramework.procDeleteCommunity(communityId);

                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.DeleteCommunityBL(): " + ex.ToString());
                return false;
            }
        }


        private void DeletePollsAssociatedWithCommunity(int communityId)
        {
            /* Because we store the different Polls (Polls for Community, PostalCode, Municipality, Region, Country) 
             * together in the same table, we prepend a string version of CommunityID with a "c_", to ensure uniqueness.*/
            var areaIdentifier = "c_" + communityId.ToString();

            var polls = entityFramework.Polls.Where(p => p.AreaIdentifier == areaIdentifier);

            List<int> pollIdsToDelete = new List<int>();

            if (polls != null)
            {            
                foreach (var p in polls)
                {
                    pollIdsToDelete.Add(p.PollID);
                }
            }
            foreach (var p in pollIdsToDelete)
                DeletePollBL(p);
        }


        public List<CommunityDto> GetCommunitiesCreatedByUserBL(int userId, int pageSize, int pageNumber)
        {
            try
            {
                var communities = entityFramework.procGetCommunitiesCreatedByUser(userId, pageSize, pageNumber);
                    
                var communityDtos = new List<CommunityDto>();

                if (communities != null)
                {
                    foreach (var c in communities)
                    {
                        var communityDto = new CommunityDto()
                        {
                            CommunityID = c.CommunityID,
                            AddedByUserID = c.AddedByUserID,
                            Name = c.Name,
                            CreateUpdateDate = c.CreateUpdateDate,
                            PolygonWkt = c.PolygonWkt,
                            HasPhoto = DoesCommunityHavePhoto(c.CommunityID),
                            LatestActivity = c.LatestActivity,
                            NumberOfUsersInCommunity = Convert.ToInt32(c.NumberOfUsers),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(c.HasNextPageOfData)
                        };

                        /* Center point of Community area. */
                        communityDto.PolygonCenterLatitude = ExtractLatitudeFromPointWkt(c.PolygonCenterPointWkt);
                        communityDto.PolygonCenterLongitude = ExtractLongitudeFromPointWkt(c.PolygonCenterPointWkt);

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like datetype DateTime. */
                        if (c.LatestActivity != null)
                            communityDto.LatestActivityToString = "Latests Activity: " + c.LatestActivity.ToString();
                        else
                            communityDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var communityPhoto = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == c.CommunityID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (communityPhoto != null)
                        {
                            communityDto.ImageBlobUri = GetSasUriForBlobReadBL(communityPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            communityDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(communityPhoto.MediumSizeBlobUri);
                            communityDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(communityPhoto.ThumbnailBlobUri);
                        }
                        communityDtos.Add(communityDto);
                    }
                    return communityDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.GetCommunitiesCreatedByUserBL(): " + ex.ToString());
                return null;
            }
        }


        public List<CommunityDto> GetCommunitiesFollowedByUserBL(int userId, int pageSize, int pageNumber)
        {
            try
            {
                var communities = entityFramework.procGetCommunitiesFollowedByUser(userId, pageSize, pageNumber);

                var communityDtos = new List<CommunityDto>();

                if (communities != null)
                {
                    foreach (var c in communities)
                    {
                        var communityDto = new CommunityDto()
                        {
                            CommunityID = c.CommunityID,
                            AddedByUserID = c.AddedByUserID,
                            Name = c.Name,
                            PolygonWkt = c.PolygonWkt,
                            HasPhoto = DoesCommunityHavePhoto(c.CommunityID),
                            LatestActivity = c.LatestActivity,
                            NumberOfUsersInCommunity = Convert.ToInt32(c.NumberOfUsers),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(c.HasNextPageOfData)
                        };

                        /* Center point of Community area. */
                        communityDto.PolygonCenterLatitude = ExtractLatitudeFromPointWkt(c.PolygonCenterPointWkt);
                        communityDto.PolygonCenterLongitude = ExtractLongitudeFromPointWkt(c.PolygonCenterPointWkt);

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like datetype DateTime. */
                        if (c.LatestActivity != null)
                            communityDto.LatestActivityToString = "Latests Activity: " + c.LatestActivity.ToString();
                        else
                            communityDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var communityPhoto = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == c.CommunityID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (communityPhoto != null)
                        {
                            communityDto.ImageBlobUri = GetSasUriForBlobReadBL(communityPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            communityDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(communityPhoto.MediumSizeBlobUri);
                            communityDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(communityPhoto.ThumbnailBlobUri);
                        }
                        communityDtos.Add(communityDto);
                    }
                    return communityDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.GetCommunitiesFollowedByUserBL(): " + ex.ToString());
                return null;
            }
        }


        public bool IsCommunityCreatedByUserBL(int userId, int communityId)
        {
            try
            {
                var userCreater = entityFramework.procIsCommunityCreatedByUser(userId, communityId).Single();

                if (userCreater > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.IsCommunityCreatedByUserBL(): " + ex.ToString());
                return false;
            }
        }


        public List<NewsItemDto> GetNewestNewsItemsFromCommunityBL(int communityId, int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewestNewsItemsFromCommunity(communityId, pageSize, pageNumber);

                var newsItemDtos = new List<NewsItemDto>();

                if (newsItems != null)
                {
                    foreach (var n in newsItems)
                    {
                        var newsItemDto = new NewsItemDto()
                        {
                            NewsItemID = n.NewsItemID,
                            PostedByUserID = n.PostedByUserID,
                            PostedByUserName = GetUserName(n.NewsItemID),
                            CategoryID = n.CategoryID,
                            CategoryName = GetNewsItemCategoryName(n.CategoryID),
                            AssignmentID = n.AssignmentID,
                            AssignmentTitle = GetAssignmentTitle(Convert.ToInt32(n.AssignmentID)),
                            Title = n.Title,
                            PositionPointWkt = n.PositionPointWkt,
                            Latitude = ExtractLatitudeFromPointWkt(n.PositionPointWkt),
                            Longitude = ExtractLongitudeFromPointWkt(n.PositionPointWkt),
                            CreateUpdateDate = n.CreateUpdateDate,
                            CreateUpdateDateToString = n.CreateUpdateDate.ToString(),
                            IsLocalBreakingNews = n.IsLocalBreakingNews,
                            NumberOfViews = n.NumberOfViews,
                            NumberOfComments = n.NumberOfComments,
                            NumberOfShares = n.NumberOfShares,
                            HasPhoto = DoesNewsItemHavePhoto(n.NewsItemID),
                            HasVideo = DoesNewsItemHaveVideo(n.NewsItemID)
                        };

                        if (n.HasNextPageOfData == "yes")
                            newsItemDto.HasNextPageOfData = true;
                        else
                            newsItemDto.HasNextPageOfData = false;

                        if (DoesNewsItemHavePhoto(n.NewsItemID))
                        {
                            var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == n.NewsItemID).First();

                            /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                            if (newsItemCoverPhoto != null)
                            {
                                newsItemDto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                                newsItemDto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                                newsItemDto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                            }
                        }
                        newsItemDtos.Add(newsItemDto);
                    }
                    return newsItemDtos;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.GetNewestNewsItemsFromCommunityBL(): " + ex.ToString());
                return null;
            }
        }


        public List<UserDto> GetLatestActiveUsersFromCommunityBL(int communityId, int pageSize, int pageNumber)
        {
            try
            {
                var users = entityFramework.procGetLatestActiveUsersFromCommunity(communityId, pageSize, pageNumber);

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
                            userDto.LatestActivityToString = "Latest Activity: " + u.LatestActivity.ToString();
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
                Trace.TraceError("Problem in CommunityBL.GetLatestActiveUsersFromCommunityBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Return a paginated list of Communities from the whole of Denmark territory. The Communities with the latest activity
        /// (Having had added NewsItems within their area latest) are in the beginning of the list.
        /// </summary>
        public List<CommunityDto> GetLatestActiveCommunitiesFromDkCountryBL(int pageSize, int pageNumber)
        {
            try
            {
                var communities = entityFramework.procGetLatestActiveCommunitiesFromDkCountry(pageSize, pageNumber);

                var communityDtos = new List<CommunityDto>();

                if (communities != null)
                {
                    foreach (var c in communities)
                    {
                        var communityDto = new CommunityDto()
                        {
                            CommunityID = c.CommunityID,
                            AddedByUserID = c.AddedByUserID,
                            Name = c.Name,
                            Description = c.Description,
                            PolygonWkt = c.PolygonWkt,
                            CreateUpdateDate = c.CreateUpdateDate,
                            HasPhoto = DoesCommunityHavePhoto(c.CommunityID),
                            LatestActivity = c.LatestActivity,
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(c.HasNextPageOfData)
                        };
                        communityDto.NumberOfUsersInCommunity = GetNumberOfUsersInCommunity(c.CommunityID);
                        communityDto.PolygonCenterLatitude = ExtractLatitudeFromPointWkt(c.PolygonCenterPointWkt);
                        communityDto.PolygonCenterLongitude = ExtractLongitudeFromPointWkt(c.PolygonCenterPointWkt);

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like data type DateTime. */
                        if (c.LatestActivity != null)
                            communityDto.LatestActivityToString = "Latest Activity: " + c.LatestActivity.ToString();
                        else
                            communityDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var communityPhoto = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == c.CommunityID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (communityPhoto != null)
                        {
                            communityDto.ImageBlobUri = GetSasUriForBlobReadBL(communityPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            communityDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(communityPhoto.MediumSizeBlobUri);
                            communityDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(communityPhoto.ThumbnailBlobUri);
                        }
                        communityDtos.Add(communityDto);
                    }
                    return communityDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.GetLatestActiveCommunitiesFromDkCountryBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Return a paginated list of Communities from a particular Region. The Communities with the latest activity
        /// (Having had added NewsItems within their area latest) are in the beginning of the list.
        /// </summary>
        public List<CommunityDto> GetLatestActiveCommunitiesFromRegionBL(string urlRegionName, int pageSize, int pageNumber)
        {
            try
            {
                var communities = entityFramework.procGetLatestActiveCommunitiesFromRegion(urlRegionName,pageSize, pageNumber);

                var communityDtos = new List<CommunityDto>();

                if (communities != null)
                {
                    foreach (var c in communities)
                    {
                        var communityDto = new CommunityDto()
                        {
                            CommunityID = c.CommunityID,
                            AddedByUserID = c.AddedByUserID,
                            Name = c.Name,
                            Description = c.Description,
                            PolygonWkt = c.PolygonWkt,
                            CreateUpdateDate = c.CreateUpdateDate,
                            HasPhoto = DoesCommunityHavePhoto(c.CommunityID),
                            LatestActivity = c.LatestActivity,
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(c.HasNextPageOfData)
                        };
                        communityDto.NumberOfUsersInCommunity = GetNumberOfUsersInCommunity(c.CommunityID);
                        communityDto.PolygonCenterLatitude = ExtractLatitudeFromPointWkt(c.PolygonCenterPointWkt);
                        communityDto.PolygonCenterLongitude = ExtractLongitudeFromPointWkt(c.PolygonCenterPointWkt);

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like datetype DateTime. */
                        if (c.LatestActivity != null)
                            communityDto.LatestActivityToString = "Latest Activity: " + c.LatestActivity.ToString();
                        else
                            communityDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var communityPhoto = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == c.CommunityID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (communityPhoto != null)
                        {
                            communityDto.ImageBlobUri = GetSasUriForBlobReadBL(communityPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            communityDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(communityPhoto.MediumSizeBlobUri);
                            communityDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(communityPhoto.ThumbnailBlobUri);
                        }
                        communityDtos.Add(communityDto);
                    }
                    return communityDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.GetLatestActiveCommunitiesFromRegionBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Return a paginated list of Communities from a particular Municipality. The Communities with the latest activity
        /// (Having had added NewsItems within their area latest) are in the beginning of the list.
        /// </summary>
        public List<CommunityDto> GetLatestActiveCommunitiesFromMunicipalityBL(string urlMunicipalityName, int pageSize, int pageNumber)
        {
            try
            {
                var communities = entityFramework.procGetLatestActiveCommunitiesFromMunicipality(urlMunicipalityName, pageSize, pageNumber);

                var communityDtos = new List<CommunityDto>();

                if (communities != null)
                {
                    foreach (var c in communities)
                    {
                        var communityDto = new CommunityDto()
                        {
                            CommunityID = c.CommunityID,
                            AddedByUserID = c.AddedByUserID,
                            Name = c.Name,
                            Description = c.Description,
                            PolygonWkt = c.PolygonWkt,
                            CreateUpdateDate = c.CreateUpdateDate,
                            HasPhoto = DoesCommunityHavePhoto(c.CommunityID),
                            LatestActivity = c.LatestActivity,
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(c.HasNextPageOfData)
                        };
                        communityDto.NumberOfUsersInCommunity = GetNumberOfUsersInCommunity(c.CommunityID);
                        communityDto.PolygonCenterLatitude = ExtractLatitudeFromPointWkt(c.PolygonCenterPointWkt);
                        communityDto.PolygonCenterLongitude = ExtractLongitudeFromPointWkt(c.PolygonCenterPointWkt);

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like datetype DateTime. */
                        if (c.LatestActivity != null)
                            communityDto.LatestActivityToString = "Latest Activity: " + c.LatestActivity.ToString();
                        else
                            communityDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var communityPhoto = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == c.CommunityID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (communityPhoto != null)
                        {
                            communityDto.ImageBlobUri = GetSasUriForBlobReadBL(communityPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            communityDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(communityPhoto.MediumSizeBlobUri);
                            communityDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(communityPhoto.ThumbnailBlobUri);
                        }
                        communityDtos.Add(communityDto);
                    }
                    return communityDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.GetLatestActiveCommunitiesFromMunicipalityBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Return a paginated list of Communities from a particular PostalCode. The Communities with the latest activity
        /// (Having had added NewsItems within their area latest) are in the beginning of the list.
        /// </summary>
        public List<CommunityDto> GetLatestActiveCommunitiesFromPostalCodeBL(string POSTNR_TXT, int pageSize, int pageNumber)
        {
            try
            {
                var communities = entityFramework.procGetLatestActiveCommunitiesFromPostalCode(POSTNR_TXT, pageSize, pageNumber);

                var communityDtos = new List<CommunityDto>();

                if (communities != null)
                {
                    foreach (var c in communities)
                    {
                        var communityDto = new CommunityDto()
                        {
                            CommunityID = c.CommunityID,
                            AddedByUserID = c.AddedByUserID,
                            Name = c.Name,
                            Description = c.Description,
                            PolygonWkt = c.PolygonWkt,
                            CreateUpdateDate = c.CreateUpdateDate,
                            HasPhoto = DoesCommunityHavePhoto(c.CommunityID),
                            LatestActivity = c.LatestActivity,
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(c.HasNextPageOfData)
                        };
                        communityDto.NumberOfUsersInCommunity = GetNumberOfUsersInCommunity(c.CommunityID);
                        communityDto.PolygonCenterLatitude = ExtractLatitudeFromPointWkt(c.PolygonCenterPointWkt);
                        communityDto.PolygonCenterLongitude = ExtractLongitudeFromPointWkt(c.PolygonCenterPointWkt);

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like datetype DateTime. */
                        if (c.LatestActivity != null)
                            communityDto.LatestActivityToString = "Latest Activity: " + c.LatestActivity.ToString();
                        else
                            communityDto.LatestActivityToString = null;

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var communityPhoto = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == c.CommunityID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (communityPhoto != null)
                        {
                            communityDto.ImageBlobUri = GetSasUriForBlobReadBL(communityPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            communityDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(communityPhoto.MediumSizeBlobUri);
                            communityDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(communityPhoto.ThumbnailBlobUri);
                        }
                        communityDtos.Add(communityDto);
                    }
                    return communityDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.GetLatestActiveCommunitiesFromPostalCodeBL(): " + ex.ToString());
                return null;
            }
        }


        public List<CommunityDto> GetAllCommunitiesBL()
        {
            try
            {
                var communities = entityFramework.procGetAllCommunities();

                var communityDtos = new List<CommunityDto>();

                if (communities != null)
                {
                    foreach (var c in communities)
                    {
                        var communityDto = new CommunityDto()
                        {
                            CommunityID = c.CommunityID,
                            Name = c.Name,
                        };

                        /* Finding and adding an eventual community photo to the CommunityDto. */
                        var communityPhoto = entityFramework.Media.OfType<CommunityPhoto>().Where(p => p.CommunityID == c.CommunityID).SingleOrDefault();
                        /* We are adding the different sizes of the community photo blob URIs to the CommunityDto. */
                        if (communityPhoto != null)
                        {
                            communityDto.ImageBlobUri = GetSasUriForBlobReadBL(communityPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            //communityDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(communityPhoto.MediumSizeBlobUri);
                            communityDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(communityPhoto.ThumbnailBlobUri);
                        }
                        communityDtos.Add(communityDto);
                    }
                    return communityDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.GetAllCommunitiesBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Checks whether a particular User follows a particular Community.
        /// </summary>
        public bool IsUserFollowingCommunityBL(int userId, int communityId)
        {
            try
            {
                var doesUser = entityFramework.procIsUserFollowingCommunity(userId, communityId).Single();

                if (doesUser > 0)
                    return true;
                else
                    return false;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.IsUserFollowingCommunityBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Makes a User follow a Community.
        /// </summary>
        public bool UserFollowCommunityBL(int userId, int communityId)
        {
            try
            {
                entityFramework.procUserFollowCommunity
                (
                    userId,
                    communityId
                );
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.UserFollowCommunityBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Makes a User unfollow a Community.
        /// </summary>
        public bool UserUnfollowCommunityBL(int userId, int communityId)
        {
            try
            {
                entityFramework.procUserUnfollowCommunity
                (
                    userId,
                    communityId
                );
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in CommunityBL.UserUnfollowCommunityBL(): " + ex.ToString());
                return false;
            }
        }
    }
}