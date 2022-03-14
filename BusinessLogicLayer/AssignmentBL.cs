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
        public AssignmentDto GetAssignmentBL(int assignmentId)
        {
            try
            {
                var assignmentDto = new AssignmentDto();

                var assignment = entityFramework.procGetAssignment(assignmentId).SingleOrDefault();

                var assignmentPhoto = entityFramework.Media.OfType<AssignmentPhoto>().Where(a => a.AssignmentID == assignmentId).SingleOrDefault();

                /* We are adding the different sizes of the photo blob URIs. */
                if (assignmentPhoto != null)
                {
                    assignmentDto.ImageBlobUri = GetSasUriForBlobReadBL(assignmentPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                    assignmentDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(assignmentPhoto.MediumSizeBlobUri);
                    assignmentDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(assignmentPhoto.ThumbnailBlobUri);
                }

                if (assignment != null)
                {
                    assignmentDto.AssignmentID = assignment.AssignmentID;
                    assignmentDto.AddedByUserID = assignment.AddedByUserID;
                    assignmentDto.Title = assignment.Title;
                    assignmentDto.Description = assignment.Description;
                    assignmentDto.CreateUpdateDate = assignment.CreateUpdateDate;
                    assignmentDto.ExpiryDate = assignment.ExpiryDate;
                    assignmentDto.HasPhoto = DoesAssignmentHavePhoto(assignment.AssignmentID);
                    assignmentDto.NumberOfNewsItemsOnAssignment = Convert.ToInt32(assignment.NumberOfNewsItemsOnAssignment);
                    assignmentDto.LatestActivity = assignment.TimeOfLatestActivityOnAssignment;
                    assignmentDto.AssignmentCenterAddress = assignment.AssignmentCenterAddress;
                    assignmentDto.AreaPolygonWkt = assignment.AreaPolygonWkt;

                    /* Adding a string version of LatestActivity. */
                    if (assignment.TimeOfLatestActivityOnAssignment != null)
                        assignmentDto.LatestActivityToString = "Latest Activity: " + assignment.TimeOfLatestActivityOnAssignment.ToString();
                    else
                        assignmentDto.LatestActivityToString = null;

                    /* Converting AssignmentAreaRadius from meters to kilometers. */
                    if (assignment.AssignmentAreaRadius != null)
                       assignmentDto.AssignmentRadius = Convert.ToInt32(assignment.AssignmentAreaRadius) / 1000;

                    return assignmentDto;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in AssignmentBL.GetAssignmentBL(): " + ex.ToString());
                return null;
            }
        }


        public int CreateAssignmentBL(AssignmentDto assignmentDto)
        {
            try
            {
                var assignment = new EntityFramework.Assignment();

                assignment.AddedByUserID = assignmentDto.AddedByUserID;
                assignment.Title = assignmentDto.Title;
                assignment.Description = assignmentDto.Description;
                assignment.CreateUpdateDate = DateTime.Now;
                assignment.ExpiryDate = Convert.ToDateTime(assignmentDto.ExpiryDate.ToString());

                entityFramework.Assignments.AddObject(assignment);
                entityFramework.SaveChanges();

                return assignment.AssignmentID;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in AssignmentBL.CreateAssignmentBL(): " + ex.ToString());
                return -1;
            }
        }



        public bool UpdateAssignmentBL(AssignmentDto assignmentDto)
        {
            try
            {
                entityFramework.procUpdateAssignment
                (
                    assignmentDto.AssignmentID,
                    assignmentDto.Title,
                    assignmentDto.Description,
                    DateTime.Now,
                    Convert.ToDateTime(assignmentDto.ExpiryDate)
                );
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in AssignmentBL.UpdateAssignmentBL(): " + ex.ToString());
                return false;
            }
        }


        public bool DeleteAssignmentBL(int assignmentId)
        {
            try
            {
                /* Deleting AssignmentPhoto */
                var assignmentPhoto = entityFramework.Media.OfType<AssignmentPhoto>().Where(a => a.AssignmentID == assignmentId).SingleOrDefault();
                if (assignmentPhoto != null)
                {
                    DeleteMediaBL(assignmentPhoto.MediaID, MediaUsage.Assignment);
                }

                /* Deleting Assignment */
                entityFramework.procDeleteAssignment(assignmentId);
                return true;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in AssignmentBL.DeleteAssignmentBL(): " + ex.ToString());
                return false;
            }
        }


        /// <summary>
        /// A GeoTemporalAssignment is an Assignment which, apart from general Assignment characteristics
        /// (inherited from Assignment), also emphasizes a spatial and temporal dimension.
        /// It alerts Users (who want to receive geo temporal assignment alerts) to participate.
        /// Those Users alerted falls in two groups:
        /// - 1. Users who we know have been in the Assignment area within a certain time frame.
        /// - 2. Users who live within the Assignment area.
        /// An example of a GeoTemporalAssignment could be @Copenhagen Central within a radius of 1 km
        /// Bomb exploded! Are you there? What's happening?
        /// </summary>
        public int CreateGeoTemporalAssignmentBL(AssignmentDto dto)
        {
            try
            {
                var createUpdateDate = DateTime.Now;
                var assignmentId = entityFramework.procCreateGeoTemporalAssignment
                    (
                        dto.AddedByUserID,
                        dto.Title,
                        dto.Description,
                        createUpdateDate,
                        dto.ExpiryDate,
                        dto.AssignmentCenterAddress,
                        ConvertLatLongToPointWkt(dto.AssignmentCenterLongitude, dto.AssignmentCenterLatitude),
                        dto.AssignmentRadius * 1000
                    ).Single();

                if (assignmentId != null)
                {
                    dto.AssignmentID = Convert.ToInt32(assignmentId);
                    dto.CreateUpdateDate = createUpdateDate;

                    /* Sends out an alert to the relevant Users within 
                     * the time and space of the geo temporal assignment. */
                    AlertUsersOnGeoTemporalAssignment(dto);

                    return Convert.ToInt32(assignmentId);
                }
                else
                {
                    return -1;
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in AssignmentBL.CreateGeoTemporalAssignmentBL(): " + ex.ToString());
                return -1;
            }
        }


        public List<AssignmentDto> GetAssignmentsForDropDownListBL()
        {
            try
            {
                var assignments = entityFramework.Assignments.Where(a => a.ExpiryDate > DateTime.Now);

                var assignmentDtos = new List<AssignmentDto>();

                if (assignments != null)
                {
                    foreach (var a in assignments)
                    {
                        assignmentDtos.Add
                        (
                            new AssignmentDto()
                            {
                                AssignmentID = a.AssignmentID,
                                AddedByUserID = a.AddedByUserID,
                                Title = a.Title,
                                Description = a.Description,
                                CreateUpdateDate = a.CreateUpdateDate,
                                ExpiryDate = a.ExpiryDate
                            }
                        );
                    }
                    return assignmentDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in AssignmentBL.GetAssignmentsBL(): " + ex.ToString());
                return null;
            }
        }


        public List<NewsItemDto> GetNewestNewsItemsOnAssignmentBL(int assignmentId, int pageSize, int pageNumber)
        {
            try
            {
                var newsItems = entityFramework.procGetNewestNewsItemsOnAssignment(assignmentId, pageSize, pageNumber);

                var newsItemDtos = new List<NewsItemDto>();

                if (newsItems != null)
                {
                    foreach (var n in newsItems)
                    {
                        var newsItemDto = new NewsItemDto()
                        {
                            NewsItemID = n.NewsItemID,
                            CategoryName = GetNewsItemCategoryName(n.CategoryID),
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
                Trace.TraceError("Problem in AssignmentBL.GetNewestNewsItemsOnAssignmentBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        /// Gets a list of the Assignments which are not expired. The newest Assignments are in the beginning of the list.
        /// </summary>
        public List<AssignmentDto> GetActiveAssignmentsBL(int pageSize, int pageNumber)
        {
            try
            {
                var assignments = entityFramework.procGetActiveAssignments(pageSize, pageNumber);

                var assignmentDtos = new List<AssignmentDto>();

                if (assignments != null)
                {
                    foreach (var a in assignments)
                    {
                        var assignmentDto = new AssignmentDto()
                        {
                            AssignmentID = a.AssignmentID,
                            AddedByUserID = a.AddedByUserID,
                            Title = a.Title,
                            Description = a.Description,
                            CreateUpdateDate = a.CreateUpdateDate,
                            ExpiryDate = a.ExpiryDate,
                            HasPhoto = DoesAssignmentHavePhoto(a.AssignmentID),
                            NumberOfNewsItemsOnAssignment = Convert.ToInt32(a.NumberOfNewsItemsOnAssignment),
                            LatestActivity = a.TimeOfLatestActivityOnAssignment,
                            AssignmentCenterAddress = a.AssignmentCenterAddress
                        };

                        /* The position where there were latest added News on the Assignment. */
                        if (a.LatestActivityPointWkt != null)
                        {
                            // The two fields below transfers the position where there were latest added News on the Assignment.
                            assignmentDto.LatestNewsLatitude = ExtractLatitudeFromPointWkt(a.LatestActivityPointWkt);
                            assignmentDto.LatestNewsLongitude = ExtractLongitudeFromPointWkt(a.LatestActivityPointWkt);
                        }
                        else
                        {
                            assignmentDto.LatestNewsLatitude = null;
                            assignmentDto.LatestNewsLongitude = null;
                        }

                        /* Adding a string version of LatestActivity. */
                        if (a.TimeOfLatestActivityOnAssignment != null)
                            assignmentDto.LatestActivityToString = "Latest Activity: " + a.TimeOfLatestActivityOnAssignment.ToString();
                        else
                            assignmentDto.LatestActivityToString = null;

                        /* Probing for if there is a next page of data beyond the current page. */
                        if (a.HasNextPageOfData == "yes")
                            assignmentDto.HasNextPageOfData = true;
                        else
                            assignmentDto.HasNextPageOfData = false;

                        if (a.AssignmentAreaRadius != null)
                            assignmentDto.AssignmentRadius = a.AssignmentAreaRadius / 1000;
                        else
                            assignmentDto.AssignmentRadius = null;

                        /* Checking for photo. */
                        var assignmentPhoto = entityFramework.Media.OfType<AssignmentPhoto>().Where(x => x.AssignmentID == a.AssignmentID).SingleOrDefault();

                        /* We are adding the different sizes of the photo blob URIs. */
                        if (assignmentPhoto != null)
                        {
                            assignmentDto.ImageBlobUri = GetSasUriForBlobReadBL(assignmentPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            assignmentDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(assignmentPhoto.MediumSizeBlobUri);
                            assignmentDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(assignmentPhoto.ThumbnailBlobUri);
                        }
                        assignmentDtos.Add(assignmentDto);
                    }
                    return assignmentDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in AssignmentBL.GetActiveAssignmentsBL(): " + ex.ToString());
                return null;
            }
        }


        public List<AssignmentDto> GetAssignmentsCreatedByUserBL(int userId, int pageSize, int pageNumber)
        {
            try
            {
                var assignments = entityFramework.procGetAssignmentsCreatedByUser(userId, pageSize, pageNumber);

                var assignmentDtos = new List<AssignmentDto>();

                if (assignments != null)
                {
                    foreach (var a in assignments)
                    {
                        var assignmentDto = new AssignmentDto()
                        {
                            AssignmentID = a.AssignmentID,
                            AddedByUserID = a.AddedByUserID,
                            Title = a.Title,
                            Description = a.Description,
                            CreateUpdateDate = a.CreateUpdateDate,
                            ExpiryDate = a.ExpiryDate,
                            HasPhoto = DoesAssignmentHavePhoto(a.AssignmentID),
                            NumberOfNewsItemsOnAssignment = Convert.ToInt32(a.NumberOfNewsItemsOnAssignment),
                            LatestActivity = a.TimeOfLatestActivityOnAssignment,
                            AssignmentCenterAddress = a.AssignmentCenterAddress
                        };

                        /* Indicates whether the ExpiryDate of the Assignment has been passed. */
                        if (a.ExpiryDate < DateTime.Now)
                            assignmentDto.IsExpired = true;
                        else
                            assignmentDto.IsExpired = false;

                        /* The position where there were latest added News on the Assignment. */
                        if (a.LatestActivityPointWkt != null)
                        {
                            // The two fields below transfers the position where there were latest added News on the Assignment.
                            assignmentDto.LatestNewsLatitude = ExtractLatitudeFromPointWkt(a.LatestActivityPointWkt);
                            assignmentDto.LatestNewsLongitude = ExtractLongitudeFromPointWkt(a.LatestActivityPointWkt);
                        }
                        else
                        {
                            assignmentDto.LatestNewsLatitude = null;
                            assignmentDto.LatestNewsLongitude = null;
                        }

                        /* Adding a string version of LatestActivity. */
                        if (a.TimeOfLatestActivityOnAssignment != null)
                            assignmentDto.LatestActivityToString = "Latest Activity: " + a.TimeOfLatestActivityOnAssignment.ToString();
                        else
                            assignmentDto.LatestActivityToString = null;

                        /* Probing for if there is a next page of data beyond the current page. */
                        if (a.HasNextPageOfData == "yes")
                            assignmentDto.HasNextPageOfData = true;
                        else
                            assignmentDto.HasNextPageOfData = false;

                        if (a.AssignmentAreaRadius != null)
                            assignmentDto.AssignmentRadius = a.AssignmentAreaRadius / 1000;
                        else
                            assignmentDto.AssignmentRadius = null;

                        /* Checking for photo. */
                        var assignmentPhoto = entityFramework.Media.OfType<AssignmentPhoto>().Where(x => x.AssignmentID == a.AssignmentID).SingleOrDefault();

                        /* We are adding the different sizes of the photo blob URIs. */
                        if (assignmentPhoto != null)
                        {
                            assignmentDto.ImageBlobUri = GetSasUriForBlobReadBL(assignmentPhoto.BlobUri); /* We must get a SAS in order to read the blobs. */
                            assignmentDto.MediumSizeBlobUri = GetSasUriForBlobReadBL(assignmentPhoto.MediumSizeBlobUri);
                            assignmentDto.ThumbnailBlobUri = GetSasUriForBlobReadBL(assignmentPhoto.ThumbnailBlobUri);
                        }
                        assignmentDtos.Add(assignmentDto);
                    }
                    return assignmentDtos;
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                Trace.TraceError("Problem in AssignmentBL.GetAssignmentsCreatedByUserBL(): " + ex.ToString());
                return null;
            }
        }


        /// <summary>
        ///   Gets Users within the area (GeoTemporalAssignments.AreaPolygon), and time (HoursToGoBack) of a geo temporal Assignment. 
        ///   The query only selects Users who have opted in on UserAlerts.AlertOnNotificationsFromEditors. 
        ///   That is Users who wants to receive notifications from Editors.
        ///   The query selects two kinds of Users who are interesting to the Assignment:
        ///   1. - Users who we know have logged in, within the Assignment area within the temporal confines, that is, Users who were
        ///   within (GeoTemporalAssignments.AreaPolygon) at the time span defined by HoursToGoBack.
        ///   2. - The other kind of Users the query selects is Users who live within the Assignment area.
        ///   For those Users who where within the Assignment area at the temporal confines we select their LastLoginPositionPoint.
        ///   For those Users who live within the Assignment area we select their AddressPositionPoint.
        ///   We naturally exclude the User who have posted the GeoTemporalAssignment.
        /// </summary>
        public List<UserDto> GetUsersWithinAreaAndTimeOfGeoTemporalAssignmentBL(AssignmentDto dto)
        {
            try
            {
                var users = entityFramework.procGetUsersWithinAreaAndTimeOfGeoTemporalAssignment
                    (
                        dto.AddedByUserID,
                        dto.AssignmentID,                      
                        dto.HoursToGoBack,
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
                            FullName = u.UserFullName,
                            // Out of convenience I am using the AddressPositionPointWkt field to transport the PositionPointWkt.
                            AddressPositionPointWkt = u.PositionPointWkt,
                            LastLoginDateTime = u.LastLoginDateTime,
                            HasPhoto = DoesUserHavePhoto(u.UserID),
                            HasNextPageOfData = ConvertHasNextPageOfDataStringToBool(u.HasNextPageOfData),
                            NumberOfSearchResults = Convert.ToInt32(u.NumberOfSearchResults)
                        };
                        /* The query returns two types of Users:
                         1. - Users who was in the area within the spatial and temporal confines. 
                         2. - Users who live in the area.*/
                        if (u.UserWasInTheArea == "yes")
                            userDto.LatestActivityToString = "was in the area. Last Login: " + userDto.LastLoginDateTime.ToString();
                        else
                            userDto.LatestActivityToString = "lives in the area. Last Login: " + userDto.LastLoginDateTime.ToString();

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
                Trace.TraceError("Problem in AssignmentBL.GetUsersWithinAreaAndTimeOfGeoTemporalAssignmentBL(): " + ex.ToString());
                return null;
            }
        }
    }
}