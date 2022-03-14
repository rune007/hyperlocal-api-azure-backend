using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.DataTransferObjects;
using HLServiceRole.EntityFramework;
using System.Diagnostics;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        /// <summary>
        /// Returns NewsItems in paginable list ordered according to how close they are to the search center.
        /// The variable daysToGoBack determines how many days we will go back in our NewsItems from the current date.
        /// </summary>
        /// <param name="daysToGoBack">How many daysToGoBack = How old can the NewsItems be</param>
        public List<NewsItemDto> GetNewsItemsClosestToPositionBL
            (double searchCenterLatitude, double searchCenterLongitude, int daysToGoBack, int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewsItemsClosestToPosition
                    (ConvertLatLongToPointWkt(searchCenterLongitude, searchCenterLatitude),
                    daysToGoBack, pageSize, pageNumber);

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
                            HasVideo = DoesNewsItemHaveVideo(n.NewsItemID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(n.HasNextPageOfData)
                        };

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
                Trace.TraceError("Problem in SearchBL.GetNewsItemsClosestToPositionBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Return a paginated list of Communities, ordered according to how close they are to the search center.
        /// </summary>
        public List<CommunityDto> GetCommunitiesClosestToPositionBL(double searchCenterLatitude, double searchCenterLongitude, int pageSize, int pageNumber)
        {
            try
            {
                var communities = entityFramework.procGetCommunitiesClosestToPosition(ConvertLatLongToPointWkt(searchCenterLongitude, searchCenterLatitude), pageSize, pageNumber);

                var communityDtos = new List<CommunityDto>();

                if (communities != null)
                {
                    foreach (var c in communities)
                    {
                        var communityDto = new CommunityDto()
                        {
                            CommunityID = c.CommunityID,
                            Name = c.Name,
                            PolygonWkt = c.PolygonWkt,
                            HasPhoto = DoesCommunityHavePhoto(c.CommunityID),
                            LatestActivity = c.LatestActivity,
                            NumberOfUsersInCommunity = Convert.ToInt32(c.NumberOfUsers),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(c.HasNextPageOfData)
                        };

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
                Trace.TraceError("Problem in SearchBL.GetCommunitiesClosestToPositionBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Return a paginated list of Users, ordered according to how close they are to the search center.
        /// </summary>
        public List<UserDto> GetUsersClosestToPositionBL(double searchCenterLatitude, double searchCenterLongitude, int pageSize, int pageNumber)
        {
            try
            {
                var users = entityFramework.procGetUsersClosestToPosition(ConvertLatLongToPointWkt(searchCenterLongitude, searchCenterLatitude), pageSize, pageNumber);

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
                            LatestActivity = u.LatestActivity,
                            NumberOfNewsItemsPostedByUser = Convert.ToInt32(u.NumberOfNewsItemsPostedByUser),
                            HasPhoto = DoesUserHavePhoto(u.UserID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(u.HasNextPageOfData)
                        };

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like data type DateTime. */
                        if (u.LatestActivity != null)
                            userDto.LatestActivityToString = "Latests Activity: " + u.LatestActivity.ToString();
                        else
                            userDto.LatestActivityToString = null;

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
                Trace.TraceError("Problem in SearchBL.GetUsersClosestToPositionBL(): " + ex.ToString());
                return null;
            }
        }


        public List<NewsItemDto> SearchNewsItemsBL(SearchNewsItemDto dto)
        {
            try
            {
                var newsItems = entityFramework.procSearchNewsItems
                    (
                        ConvertLatLongToPointWkt(dto.SearchCenterLongitude, dto.SearchCenterLatitude),
                        dto.SearchRadius * 1000,
                        dto.CategoryID,
                        dto.AssignmentID,
                        dto.Title,
                        dto.Story,
                        dto.CreateUpdateDate,
                        dto.PageSize,
                        dto.PageNumber
                    );

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
                            HasVideo = DoesNewsItemHaveVideo(n.NewsItemID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(n.HasNextPageOfData),
                            NumberOfSearchResults = Convert.ToInt32(n.NumberOfSearchResults)
                        };

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
                Trace.TraceError("Problem in SearchBL.SearchNewsItemsBL(): " + ex.ToString());
                return null;
            }
        }


        public List<CommunityDto> SearchCommunitiesBL(SearchCommunityDto dto)
        {
            try
            {
                var communities = entityFramework.procSearchCommunities
                    (
                        ConvertLatLongToPointWkt(dto.SearchCenterLongitude, dto.SearchCenterLatitude),
                        dto.SearchRadius * 1000,
                        dto.Name,
                        dto.Description,
                        dto.PageSize,
                        dto.PageNumber
                    );

                var communityDtos = new List<CommunityDto>();

                if (communities != null)
                {
                    foreach (var c in communities)
                    {
                        var communityDto = new CommunityDto()
                        {
                            CommunityID = c.CommunityID,
                            Name = c.Name,
                            PolygonWkt = c.PolygonWkt,
                            HasPhoto = DoesCommunityHavePhoto(c.CommunityID),
                            PolygonCenterLatitude = ExtractLatitudeFromPointWkt(c.PolygonCenterPointWkt),
                            PolygonCenterLongitude = ExtractLongitudeFromPointWkt(c.PolygonCenterPointWkt),
                            LatestActivity = c.LatestActivity,
                            NumberOfUsersInCommunity = Convert.ToInt32(c.NumberOfUsers),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(c.HasNextPageOfData),
                            NumberOfSearchResults = Convert.ToInt32(c.NumberOfSearchResults)
                        };

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
                Trace.TraceError("Problem in SearchBL.SearchCommunitiesBL(): " + ex.ToString());
                return null;
            }
        }


        public List<UserDto> SearchUsersBL(SearchUserDto dto)
        {
            try
            {
                var users = entityFramework.procSearchUsers
                    (
                        ConvertLatLongToPointWkt(dto.SearchCenterLongitude, dto.SearchCenterLatitude),
                        dto.SearchRadius * 1000,
                        dto.FirstName,
                        dto.LastName,
                        dto.Bio,
                        dto.Email,
                        dto.Address,
                        dto.Phone,
                        dto.PageSize,
                        dto.PageNumber
                    );

                var userDtos = new List<UserDto>();

                if (users != null)
                {
                    foreach (var u in users)
                    {
                        var userDto = new UserDto()
                        {
                            UserID = u.UserID,
                            FirstName = u.FirstName,
                            FullName = u.FirstName + " " + u.LastName,
                            AddressPositionPointWkt = u.AddressPositionPointWkt,
                            Latitude = ExtractLatitudeFromPointWkt(u.AddressPositionPointWkt),
                            Longitude = ExtractLongitudeFromPointWkt(u.AddressPositionPointWkt),
                            LatestActivity = u.LatestActivity,
                            NumberOfNewsItemsPostedByUser = Convert.ToInt32(u.NumberOfNewsItemsPostedByUser),
                            HasPhoto = DoesUserHavePhoto(u.UserID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(u.HasNextPageOfData),
                            NumberOfSearchResults = Convert.ToInt32(u.NumberOfSearchResults)
                        };

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like data type DateTime. */
                        if (u.LatestActivity != null)
                            userDto.LatestActivityToString = "Latests Activity: " + u.LatestActivity.ToString();
                        else
                            userDto.LatestActivityToString = null;

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
                Trace.TraceError("Problem in SearchBL.SearchUsersBL(): " + ex.ToString());
                return null;
            }
        }
    }
}