using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HLServiceRole.EntityFramework;
using HLServiceRole.DataTransferObjects;
using System.Diagnostics;
using System.Data.Objects;

namespace HLServiceRole.BusinessLogicLayer
{
    public partial class BusinessLogic
    {
        public List<NewsItemCategoryDto> GetNewsItemCategoriesBL()
        {
            try
            {
                var categories = entityFramework.NewsItemCategories;

                categories.OrderBy(c => c.CategoryName);

                var categoryDtos = new List<NewsItemCategoryDto>();

                foreach (var c in categories)
                {
                    categoryDtos.Add
                    (
                        new NewsItemCategoryDto()
                        {
                            CategoryID = c.CategoryID,
                            CategoryName = c.CategoryName
                        }
                    );
                }
                return categoryDtos;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemCategoryBL.GetNewsItemCategoriesBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// This method returns data used for giving an overview of the different NewsItemCategories. Besides data about
        /// each NewsItemCategory it also yields data about the latest NewsItem added in each NewsItemCategory.
        /// </summary>
        public List<NewsItemCategoryIndexDto> GetNewsItemCategoriesIndexBL()
        {
            try
            {
                var categories = entityFramework.procGetNewsItemCategoriesIndex();

                var categoryDtos = new List<NewsItemCategoryIndexDto>();

                if (categories != null)
                {
                    foreach (var c in categories)
                    {
                        var categoryDto = new NewsItemCategoryIndexDto()
                        {
                            CategoryID = c.CategoryID,
                            CategoryName = c.CategoryName,
                            NumberOfNewsItemsInCategory = Convert.ToInt32(c.NumberOfNewsItemsInCategory)
                        };

                        /* Adding a string version of LatestActivity, for displaying at Bing map pushpin pop-ups, which don't like datetype DateTime. */
                        if (c.LatestNewsItemIdInCategory != null)
                        {
                            /* Getting the latest NewsItem for each NewsItemCategory. */
                            var latestNewsItem = GetNewsItemBL(Convert.ToInt32(c.LatestNewsItemIdInCategory));

                            categoryDto.NewsItemID = c.LatestNewsItemIdInCategory;
                            categoryDto.Title = latestNewsItem.Title;
                            categoryDto.Story = latestNewsItem.Story;
                            categoryDto.PositionPointWkt = latestNewsItem.PositionPointWkt;
                            categoryDto.Latitude = ExtractLatitudeFromPointWkt(latestNewsItem.PositionPointWkt);
                            categoryDto.Longitude = ExtractLongitudeFromPointWkt(latestNewsItem.PositionPointWkt);
                            categoryDto.CreateUpdateDate = latestNewsItem.CreateUpdateDate;
                            categoryDto.HasPhoto = DoesNewsItemHavePhoto(latestNewsItem.NewsItemID);

                            categoryDto.CreateUpdateDateToString = "Latests Activity: " + latestNewsItem.CreateUpdateDate.ToString();

                            if (DoesNewsItemHavePhoto(latestNewsItem.NewsItemID))
                            {
                                var newsItemCoverPhoto = (NewsItemPhoto)entityFramework.Media.OfType<NewsItemPhoto>().Where(p => p.NewsItemID == latestNewsItem.NewsItemID).First();

                                /* We are adding the different sizes of the photo blob URIs to the Dto. */
                                if (newsItemCoverPhoto != null)
                                {
                                    categoryDto.ConverPhotoLarge = GetSasUriForBlobReadBL(newsItemCoverPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                                    categoryDto.CoverPhotoMediumSize = GetSasUriForBlobReadBL(newsItemCoverPhoto.MediumSizeBlobUri);
                                    categoryDto.CoverPhotoThumbNail = GetSasUriForBlobReadBL(newsItemCoverPhoto.ThumbnailBlobUri);
                                }
                            }
                        }
                        else
                            categoryDto.CreateUpdateDateToString = null;

                        categoryDtos.Add(categoryDto);
                    }
                    return categoryDtos;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemCategoryBL.GetNewsItemCategoriesIndexBL(): " + ex.ToString());
                return null;
            }
        }


        public NewsItemCategoryIndexDto GetNewsItemCategoryBL(int categoryId)
        {
            try
            {
                var categoryDto = new NewsItemCategoryIndexDto();

                var category = entityFramework.procGetNewsItemCategory(categoryId).SingleOrDefault();

                if (category != null)
                {
                    categoryDto.CategoryID = category.CategoryID;
                    categoryDto.CategoryName = category.CategoryName;
                    /* CreateUpdateDate is the time when a NewsItem latest was added in a NewsItemCategory. 
                     We have kept the field name "CreateUpdateDate" from NewsItem where is derives from. */
                    categoryDto.CreateUpdateDate = Convert.ToDateTime(category.LatestActivity);
                    categoryDto.NumberOfNewsItemsInCategory = Convert.ToInt32(category.NumberOfNewsItemsInCategory);

                    if (category.LatestActivity != null)
                        categoryDto.CreateUpdateDateToString = "Latests Activity: " + category.LatestActivity.ToString();
                    else
                        categoryDto.CreateUpdateDateToString = null;

                    return categoryDto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in NewsItemCategoryBL.GetNewsItemCategoryBL(): " + ex.ToString());
                return null;
            }
        }


        public List<NewsItemDto> GetNewestNewsItemsInCategoryBL(int categoryId, int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewestNewsItemsInCategory(categoryId, pageSize, pageNumber);

                var newsItemDtos = new List<NewsItemDto>();

                if (newsItems != null)
                {
                    foreach (var n in newsItems)
                    {
                        var newsItemDto = new NewsItemDto()
                        {
                            NewsItemID = n.NewsItemID,
                            Title = n.Title,
                            Story = n.Story,
                            PostedByUserID = n.PostedByUserID,
                            PostedByUserName = n.PostedByUserFullName,
                            AssignmentID = n.AssignmentID,
                            AssignmentTitle = n.AssignmentTitle,
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
                Trace.TraceError("Problem in NewsItemCategoryBL.GetNewestNewsItemsInCategoryBL(): " + ex.ToString());
                return null;
            }
        }


        private string GetNewsItemCategoryName(int categoryId)
        {
            try
            {
                var category = entityFramework.NewsItemCategories.Where(c => c.CategoryID == categoryId).SingleOrDefault();

                if (category != null)
                {
                    return category.CategoryName;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in BusinessLogic.GetNewsItemCategoryName(): " + ex.ToString());
                return null;
            }
        }
    }
}