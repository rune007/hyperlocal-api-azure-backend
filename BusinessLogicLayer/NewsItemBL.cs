using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.EntityFramework;
using HLServiceRole.DataTransferObjects;
using System.Diagnostics;
using System.Text.RegularExpressions;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        public NewsItemDto GetNewsItemBL(int newsItemId)
        {
            try
            {
                var newsItem = entityFramework.procGetNewsItem(newsItemId).SingleOrDefault();

                if (newsItem != null)
                {
                    return new NewsItemDto()
                    {
                        NewsItemID = newsItem.NewsItemID,
                        PostedByUserID = newsItem.PostedByUserID,
                        PostedByUserName = GetUserName(newsItem.PostedByUserID),
                        CategoryID = newsItem.CategoryID,
                        CategoryName = GetNewsItemCategoryName(newsItem.CategoryID),
                        AssignmentID = newsItem.AssignmentID,
                        AssignmentTitle = GetAssignmentTitle(Convert.ToInt32(newsItem.AssignmentID)),
                        Title = newsItem.Title,
                        Story = newsItem.Story,
                        PositionPointWkt = newsItem.PositionPointWkt,
                        Latitude = Convert.ToDouble(ExtractLatitudeFromPointWkt(newsItem.PositionPointWkt)),
                        Longitude = Convert.ToDouble(ExtractLongitudeFromPointWkt(newsItem.PositionPointWkt)),
                        CreateUpdateDate = newsItem.CreateUpdateDate,
                        IsLocalBreakingNews = newsItem.IsLocalBreakingNews,
                        Photos = GetNewsItemPhotosBL(newsItemId),
                        Videos = GetNewsItemVideosBL(newsItemId),
                        NumberOfViews = newsItem.NumberOfViews,
                        NumberOfComments = newsItem.NumberOfComments,
                        NumberOfShares = newsItem.NumberOfShares,
                        HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID),
                        HasVideo = DoesNewsItemHaveVideo(newsItem.NewsItemID)
                    };
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.GetNewsItemBL(): " + ex.ToString());
                return null;
            }
        }


        public int CreateNewsItemBL(NewsItemDto newsItemDto)
        {
            try
            {
                var newsItemId = entityFramework.procAddNewsItem
                (
                    newsItemDto.PostedByUserID,
                    newsItemDto.CategoryID,
                    newsItemDto.AssignmentID,
                    newsItemDto.Title,
                    newsItemDto.Story,
                    ConvertLatLongToPointWkt(newsItemDto.Longitude, newsItemDto.Latitude),
                    DateTime.Now,
                    newsItemDto.IsLocalBreakingNews,
                    // We set NumberOfViews to - 1, so as to not count the redirection to Details view, after creation, as a view count. Every time NewItem is 
                    // retrieved the NumberOfViews will be incremented, thus the redirection to Details view after creation will show a correct NumberOfViews as 0.
                    -1,
                    0,
                    0
                ).Single();

                AlertUsersOnNewsItemPosted(Convert.ToInt32(newsItemId));

                return Convert.ToInt32(newsItemId);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.CreateNewsItemBL(): " + ex.ToString());
                return -1;
            }
        }


        public bool UpdateNewsItemBL(NewsItemDto newsItemDto)
        {
            try
            {
                entityFramework.procUpdateNewsItem
                (
                    newsItemDto.NewsItemID,
                    newsItemDto.CategoryID,
                    newsItemDto.AssignmentID,
                    newsItemDto.Title,
                    newsItemDto.Story,
                    ConvertLatLongToPointWkt(newsItemDto.Longitude, newsItemDto.Latitude),
                    DateTime.Now,
                    newsItemDto.IsLocalBreakingNews
                );
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.UpdateNewsItemBL(): " + ex.ToString());
                return false;
            }
        }


        public bool DeleteNewsItemBL(int newsItemId)
        {
            try
            {
                /* Deleting photo and video associated with the NewsItem. */
                DeleteMediaAssociatedWithNewsItem(newsItemId);
                /* Deletes the Comments on the NewsItem. */
                DeleteCommentsOnNewsItem(newsItemId);
                /* Deletes the NewsItem. */
                entityFramework.procDeleteNewsItem(newsItemId);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.DeleteNewsItemBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// Deletes the NewsItemPhotos and NewsItemVideos associated with a NewsItem.
        /// </summary>
        private void DeleteMediaAssociatedWithNewsItem(int newsItemId)
        {
            try
            {
                /* Deleting NewsItemPhotos*/
                var photos = entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItemId);
                List<int> photoMediaIdsToDelete = new List<int>();
                if (photos != null)
                {
                    foreach (var p in photos)
                    {
                        photoMediaIdsToDelete.Add(p.MediaID);
                    }
                }
                foreach (var p in photoMediaIdsToDelete)
                    DeleteMediaBL(p, MediaUsage.News);

                /* Deleting NewsItemVideos*/
                var videos = entityFramework.Media.OfType<NewsItemVideo>().Where(v => v.NewsItemID == newsItemId);
                List<int> videoMediaIdsToDelete = new List<int>();
                if (videos != null)
                {
                    foreach (var v in videos)
                    {
                        videoMediaIdsToDelete.Add(v.MediaID);
                    }
                }
                foreach (var v in videoMediaIdsToDelete)
                    DeleteMediaBL(v, MediaUsage.News);
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.DeleteMediaAssociatedWithNewsItem(): " + ex.ToString());
            }
        }


        public List<NewsItemDto> GetNewestNewsItemsFromDkCountryBL(int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewestNewsItemsFromDkCountry(pageSize, pageNumber);

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
                Trace.TraceError("Problem in NewsItemBL.GetNewestNewsItemsFromDkCountryBL(): " + ex.ToString());
                return null;
            }
        }


        public List<NewsItemDto> GetNewestNewsItemsFromRegionBL(string urlRegionName, int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewestNewsItemsFromRegion(urlRegionName, pageSize, pageNumber);

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
                Trace.TraceError("Problem in NewsItemBL.GetNewestNewsItemsFromRegionBL(): " + ex.ToString());
                return null;
            }
        }


        public List<NewsItemDto> GetNewestNewsItemsFromMunicipalityBL(string urlMunicipalityName, int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewestNewsItemsFromMunicipality(urlMunicipalityName, pageSize, pageNumber);

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
                Trace.TraceError("Problem in NewsItemBL.GetNewestNewsItemsFromMunicipalityBL(): " + ex.ToString());
                return null;
            }
        }



        public List<NewsItemDto> GetNewestNewsItemsFromPostalCodeBL(string POSTNR_TXT, int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewestNewsItemsFromPostalCode(POSTNR_TXT, pageSize, pageNumber);

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
                Trace.TraceError("Problem in NewsItemBL.GetNewestNewsItemsFromPostalCodeBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the news stream from the Communities which a particular User is following.
        /// </summary>
        public List<NewsItemDto> GetNewsStreamForUserBL(int userId, int daysToGoBack, int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewsStreamForUser
                    (
                        userId,
                        daysToGoBack,
                        pageSize,
                        pageNumber
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
                            PostedByUserName = n.PostedByUserFullName,
                            CategoryID = n.CategoryID,
                            CategoryName = n.CategoryName,
                            AssignmentID = n.AssignmentID,
                            AssignmentTitle = n.AssignmentTitle,
                            LocatedInCommunityID = Convert.ToInt32(n.CommunityID),
                            LocatedInCommunityName = n.CommunityName,
                            Title = n.Title,
                            Story = n.Story,
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
                Trace.TraceError("Problem in NewsItemBL.GetNewsStreamForUserBL(): " + ex.ToString());
                return null;
            }
        }


        public List<NewsItemDto> GetNewsItemsCreatedByUserBL(int userId, int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewsItemsCreatedByUser
                    (
                        userId,
                        pageSize,
                        pageNumber
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
                            CategoryID = n.CategoryID,
                            CategoryName = n.CategoryName,
                            AssignmentID = n.AssignmentID,
                            AssignmentTitle = n.AssignmentTitle,
                            Title = n.Title,
                            Story = n.Story,
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
                            NumberOfNewsItemsCreatedByUser = Convert.ToInt32(n.NumberOfNewsItemsCreatedByUser)
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
                Trace.TraceError("Problem in NewsItemBL.GetNewsItemsCreatedByUserBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the latest breaking NewsItem (Marked as IsLocalBreakingNews) from within the area of a
        /// particular Community and which is not older than hoursToGoBack subtracted from the current time.
        /// </summary>
        public NewsItemDto GetBreakingNewsFromCommunityBL(int communityId, int hoursToGoBack)
        {
            try
            {
                var newsItem = entityFramework.procGetBreakingNewsFromCommunity(communityId, hoursToGoBack).SingleOrDefault();

                var dto = new NewsItemDto();
                if (newsItem != null)
                {
                    dto.NewsItemID = newsItem.NewsItemID;
                    dto.Title = newsItem.Title;
                    dto.CreateUpdateDate = newsItem.CreateUpdateDate;
                    dto.HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID);

                    if (dto.HasPhoto)
                    {
                        var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItem.NewsItemID).First();
                        /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                        if (newsItemCoverPhoto != null)
                        {
                            dto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            dto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                            dto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                        }
                    }
                    return dto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.GetBreakingNewsFromCommunityBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Compares the current breaking news from the server with the one we display in the client. If it's the same we just return null.
        /// But if it's not the same, we return this latest breaking news to the client.
        /// </summary>
        public NewsItemDto PollingForLatestBreakingNewsFromCommunityBL(int currentBreakingNewsItemId, int communityId, int hoursToGoBack)
        {
            try
            {
                var newsItem = entityFramework.procGetBreakingNewsFromCommunity(communityId, hoursToGoBack).SingleOrDefault();
                var dto = new NewsItemDto();
                if (newsItem != null)
                {
                    /* If the NewsItem is the same as the one we are already displaying as breaking news, we are not interested and just return null. */
                    if (newsItem.NewsItemID == currentBreakingNewsItemId)
                        return null;
                    /* If it's not the same, we are very interested and return this newer breaking news to the client. */
                    else
                    {
                        dto.NewsItemID = newsItem.NewsItemID;
                        dto.Title = newsItem.Title;
                        dto.CreateUpdateDate = newsItem.CreateUpdateDate;
                        dto.HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID);

                        if (dto.HasPhoto)
                        {
                            var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItem.NewsItemID).First();
                            /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                            if (newsItemCoverPhoto != null)
                            {
                                dto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                                dto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                                dto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                            }
                        }
                        return dto;
                    }
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.PollingForLatestBreakingNewsFromCommunityBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the latest breaking NewsItem (Marked as IsLocalBreakingNews) from within the area of a
        /// particular PostalCode and which is not older than hoursToGoBack subtracted from the current time.
        /// </summary>
        public NewsItemDto GetBreakingNewsFromPostalCodeBL(string POSTNR_TXT, int hoursToGoBack)
        {
            try
            {
                var newsItem = entityFramework.procGetBreakingNewsFromPostalCode(POSTNR_TXT, hoursToGoBack).SingleOrDefault();;

                var dto = new NewsItemDto();
                if (newsItem != null)
                {
                    dto.NewsItemID = newsItem.NewsItemID;
                    dto.Title = newsItem.Title;
                    dto.CreateUpdateDate = newsItem.CreateUpdateDate;
                    dto.HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID);

                    if (dto.HasPhoto)
                    {
                        var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItem.NewsItemID).First();
                        /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                        if (newsItemCoverPhoto != null)
                        {
                            dto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            dto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                            dto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                        }
                    }
                    return dto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.GetBreakingNewsFromPostalCodeBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Compares the current breaking news from the server with the one we display in the client. If it's the same we just return null.
        /// But if it's not the same, we return this latest breaking news to the client.
        /// </summary>
        public NewsItemDto PollingForLatestBreakingNewsFromPostalCodeBL(int currentBreakingNewsItemId, string POSTNR_TXT, int hoursToGoBack)
        {
            try
            {
                var newsItem = entityFramework.procGetBreakingNewsFromPostalCode(POSTNR_TXT, hoursToGoBack).SingleOrDefault();
                var dto = new NewsItemDto();
                if (newsItem != null)
                {
                    /* If the NewsItem is the same as the one we are already displaying as breaking news, we are not interested and just return null. */
                    if (newsItem.NewsItemID == currentBreakingNewsItemId)
                        return null;
                    /* If it's not the same, we are very interested and return this newer breaking news to the client. */
                    else
                    {
                        dto.NewsItemID = newsItem.NewsItemID;
                        dto.Title = newsItem.Title;
                        dto.CreateUpdateDate = newsItem.CreateUpdateDate;
                        dto.HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID);

                        if (dto.HasPhoto)
                        {
                            var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItem.NewsItemID).First();
                            /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                            if (newsItemCoverPhoto != null)
                            {
                                dto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                                dto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                                dto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                            }
                        }
                        return dto;
                    }
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.PollingForLatestBreakingNewsFromPostalCodeBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the latest breaking NewsItem (Marked as IsLocalBreakingNews) from within the areas of the Communities that a
        /// User is following (the Users NewsStream) and which is not older than hoursToGoBack subtracted from the current time.
        /// </summary>
        public NewsItemDto GetBreakingNewsFromNewsStreamBL(int userId, int hoursToGoBack)
        {
            try
            {
                var newsItem = entityFramework.procGetBreakingNewsFromNewsStream(userId, hoursToGoBack).SingleOrDefault();

                var dto = new NewsItemDto();
                if (newsItem != null)
                {
                    dto.NewsItemID = newsItem.NewsItemID;
                    dto.Title = newsItem.Title;
                    dto.CreateUpdateDate = newsItem.CreateUpdateDate;
                    dto.HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID);

                    if (dto.HasPhoto)
                    {
                        var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItem.NewsItemID).First();
                        /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                        if (newsItemCoverPhoto != null)
                        {
                            dto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            dto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                            dto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                        }
                    }
                    return dto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.GetBreakingNewsFromNewsStreamBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Compares the current breaking news from the server with the one we display in the client. If it's the same we just return null.
        /// But if it's not the same, we return this latest breaking news to the client.
        /// A Users news stream is the stream of news comming from the Communities that they follow.
        /// </summary>
        public NewsItemDto PollingForLatestBreakingNewsFromNewsStreamBL(int currentBreakingNewsItemId, int userId, int hoursToGoBack)
        {
            try
            {
                var newsItem = entityFramework.procGetBreakingNewsFromNewsStream(userId, hoursToGoBack).SingleOrDefault();
                var dto = new NewsItemDto();
                if (newsItem != null)
                {
                    /* If the NewsItem is the same as the one we are already displaying as breaking news, we are not interested and just return null. */
                    if (newsItem.NewsItemID == currentBreakingNewsItemId)
                        return null;
                    /* If it's not the same, we are very interested and return this newer breaking news to the client. */
                    else
                    {
                        dto.NewsItemID = newsItem.NewsItemID;
                        dto.Title = newsItem.Title;
                        dto.CreateUpdateDate = newsItem.CreateUpdateDate;
                        dto.HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID);

                        if (dto.HasPhoto)
                        {
                            var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItem.NewsItemID).First();
                            /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                            if (newsItemCoverPhoto != null)
                            {
                                dto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                                dto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                                dto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                            }
                        }
                        return dto;
                    }
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.PollingForLatestBreakingNewsFromNewsStreamBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the NewsItem which have generated the most User interaction (in the way of
        /// NumberOfComments and NumberOfShares - social media sharings) from within the area 
        /// of Denmark, in the time frame defined by hoursToGoBack
        /// </summary>
        public NewsItemDto GetTrendingNewsFromDkCountryBL(int hoursToGoBack)
        {
            try
            {
                var newsItem = entityFramework.procGetTrendingNewsFromDkCountry(hoursToGoBack).SingleOrDefault();

                var dto = new NewsItemDto();
                if (newsItem != null)
                {
                    dto.NewsItemID = newsItem.NewsItemID;
                    dto.Title = newsItem.Title;
                    dto.CreateUpdateDate = newsItem.CreateUpdateDate;
                    dto.HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID);

                    if (dto.HasPhoto)
                    {
                        var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItem.NewsItemID).First();
                        /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                        if (newsItemCoverPhoto != null)
                        {
                            dto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            dto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                            dto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                        }
                    }
                    return dto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.GetTrendingNewsFromDkCountryBL(): " + ex.ToString());
                return null;
            }
        }



        /// <summary>
        /// Gets the NewsItem which have generated the most User interaction (in the way of
        /// NumberOfComments and NumberOfShares - social media sharings) from within the area 
        /// of a Region, in the time frame defined by hoursToGoBack
        /// </summary>
        public NewsItemDto GetTrendingNewsFromRegionBL(string urlRegionName, int hoursToGoBack)
        {
            try
            {
                var newsItem = entityFramework.procGetTrendingNewsFromRegion(urlRegionName, hoursToGoBack).SingleOrDefault();

                var dto = new NewsItemDto();
                if (newsItem != null)
                {
                    dto.NewsItemID = newsItem.NewsItemID;
                    dto.Title = newsItem.Title;
                    dto.CreateUpdateDate = newsItem.CreateUpdateDate;
                    dto.HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID);

                    if (dto.HasPhoto)
                    {
                        var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItem.NewsItemID).First();
                        /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                        if (newsItemCoverPhoto != null)
                        {
                            dto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            dto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                            dto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                        }
                    }
                    return dto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.GetTrendingNewsFromRegionBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets the NewsItem which have generated the most User interaction (in the way of
        /// NumberOfComments and NumberOfShares - social media sharings) from within the area 
        /// of a Municipality, in the time frame defined by hoursToGoBack
        /// </summary>
        public NewsItemDto GetTrendingNewsFromMunicipalityBL(string urlMunicipalityName, int hoursToGoBack)
        {
            try
            {
                var newsItem = entityFramework.procGetTrendingNewsFromMunicipality(urlMunicipalityName, hoursToGoBack).SingleOrDefault();

                var dto = new NewsItemDto();
                if (newsItem != null)
                {
                    dto.NewsItemID = newsItem.NewsItemID;
                    dto.Title = newsItem.Title;
                    dto.CreateUpdateDate = newsItem.CreateUpdateDate;
                    dto.HasPhoto = DoesNewsItemHavePhoto(newsItem.NewsItemID);

                    if (dto.HasPhoto)
                    {
                        var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == newsItem.NewsItemID).First();
                        /* We are adding the different sizes of the photo blob URIs to the NewsItemDto. */
                        if (newsItemCoverPhoto != null)
                        {
                            dto.CoverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            dto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                            dto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                        }
                    }
                    return dto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.GetTrendingNewsFromMunicipalityBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Increments the NumberOfShares, the number of times the NewsItem has been shared on social media.
        /// </summary>
        public bool IncrementNumberOfSharesOfNewsItemBL(int newsItemId)
        {
            try
            {
                entityFramework.procIncrementNumberOfShares(newsItemId);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemBL.IncrementNumberOfSharesOfNewsItemBL(): " + ex.ToString());
                return false;
            }
        }
    }
}