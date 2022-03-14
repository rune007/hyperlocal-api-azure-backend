using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using HLServiceRole.BusinessLogicLayer;
using HLServiceRole.DataTransferObjects;

namespace HLServiceRole
{
    /// <summary>
    /// The methods are gathered in regions which are ordered alphabetical.
    /// </summary>
    public class HLService : IHLService
    {
        BusinessLogic businessLogic;

        public HLService()
        {
            businessLogic = new BusinessLogic();
        }


        #region Assignment Methods

        public AssignmentDto GetAssignment(int assignmentId)
        {
            return businessLogic.GetAssignmentBL(assignmentId);
        }


        public int CreateAssignment(AssignmentDto assignmentDto)
        {
            return businessLogic.CreateAssignmentBL(assignmentDto);
        }


        public bool UpdateAssignment(AssignmentDto assignmentDto)
        {
            return businessLogic.UpdateAssignmentBL(assignmentDto);
        }


        public bool DeleteAssignment(int assignmentId)
        {
            return businessLogic.DeleteAssignmentBL(assignmentId);
        }


        public int CreateGeoTemporalAssignment(AssignmentDto dto)
        {
            return businessLogic.CreateGeoTemporalAssignmentBL(dto);
        }


        public List<AssignmentDto> GetAssignmentsForDropDownList()
        {
            return businessLogic.GetAssignmentsForDropDownListBL();
        }


        public List<NewsItemDto> GetNewestNewsItemsOnAssignment(int assignmentId, int pageSize, int pageNumber)
        {
            return businessLogic.GetNewestNewsItemsOnAssignmentBL(assignmentId, pageSize, pageNumber);
        }


        public List<AssignmentDto> GetActiveAssignments(int pageSize, int pageNumber)
        {
            return businessLogic.GetActiveAssignmentsBL(pageSize, pageNumber);
        }


        public List<AssignmentDto> GetAssignmentsCreatedByUser(int userId, int pageSize, int pageNumber)
        {
            return businessLogic.GetAssignmentsCreatedByUserBL(userId, pageSize, pageNumber);
        }


        public List<UserDto> GetUsersWithinAreaAndTimeOfGeoTemporalAssignment(AssignmentDto dto)
        {
            return businessLogic.GetUsersWithinAreaAndTimeOfGeoTemporalAssignmentBL(dto);
        }

        #endregion


        #region AzureBlob Methods

        public string GetSasUriForBlobWrite(BusinessLogic.MediaUsage mediaUsage, string fileName)
        {
            return businessLogic.GetSasUriForBlobWriteBL(mediaUsage, fileName);
        }

        public string GetSasUriForBlobRead(string blobUri)
        {
            return businessLogic.GetSasUriForBlobReadBL(blobUri);
        }


        public bool SaveImage(int salesItemId, string contentType, byte[] photo)
        {
            return businessLogic.SaveImageBL(salesItemId, contentType, photo);
        }

        #endregion


        #region Comment Methods

        public CommentDto CreateComment(int newsItemId, int userId, string commentBody)
        {
            return businessLogic.CreateCommentBL(newsItemId, userId, commentBody);
        }


        public bool DeleteComment(int newsItemId, string rowKey)
        {
            return businessLogic.DeleteCommentBL(newsItemId, rowKey);
        }


        public List<CommentDto> GetCommentsOnNewsItem(int newsItemId, int pageSize, int pageNumber)
        {
            return businessLogic.GetCommentsOnNewsItemBL(newsItemId, pageSize, pageNumber);
        }

        #endregion


        #region Community Methods

        public CommunityDto GetCommunity(int communityId)
        {
            return businessLogic.GetCommunityBL(communityId);
        }


        public int CreateCommunity(CommunityDto communityDto)
        {
            return businessLogic.CreateCommunityBL(communityDto);
        }


        public bool UpdateCommunity(CommunityDto communityDto)
        {
            return businessLogic.UpdateCommunityBL(communityDto);
        }


        public bool DeleteCommunity(int communityId)
        {
            return businessLogic.DeleteCommunityBL(communityId);
        }


        public List<CommunityDto> GetCommunitiesCreatedByUser(int userId, int pageSize, int pageNumber)
        {
            return businessLogic.GetCommunitiesCreatedByUserBL(userId, pageSize, pageNumber);
        }


        public List<CommunityDto> GetCommunitiesFollowedByUser(int userId, int pageSize, int pageNumber)
        {
            return businessLogic.GetCommunitiesFollowedByUserBL(userId, pageSize, pageNumber);
        }


        public bool IsCommunityCreatedByUser(int userId, int communityId)
        {
            return businessLogic.IsCommunityCreatedByUserBL(userId, communityId);
        }


        public List<NewsItemDto> GetNewestNewsItemsFromCommunity(int communityId, int pageSize, int pageNumber)
        {
            return businessLogic.GetNewestNewsItemsFromCommunityBL(communityId, pageSize, pageNumber);
        }


        public List<UserDto> GetLatestActiveUsersFromCommunity(int communityId, int pageSize, int pageNumber)
        {
            return businessLogic.GetLatestActiveUsersFromCommunityBL(communityId, pageSize, pageNumber);
        }


        public List<CommunityDto> GetLatestActiveCommunitiesFromDkCountry(int pageSize, int pageNumber)
        {
            return businessLogic.GetLatestActiveCommunitiesFromDkCountryBL(pageSize, pageNumber);
        }


        public List<CommunityDto> GetLatestActiveCommunitiesFromRegion(string urlRegionName, int pageSize, int pageNumber)
        {
            return businessLogic.GetLatestActiveCommunitiesFromRegionBL(urlRegionName, pageSize, pageNumber);
        }


        public List<CommunityDto> GetLatestActiveCommunitiesFromMunicipality(string urlMunicipalityName, int pageSize, int pageNumber)
        {
            return businessLogic.GetLatestActiveCommunitiesFromMunicipalityBL(urlMunicipalityName, pageSize, pageNumber);
        }


        public List<CommunityDto> GetLatestActiveCommunitiesFromPostalCode(string POSTNR_TXT, int pageSize, int pageNumber)
        {
            return businessLogic.GetLatestActiveCommunitiesFromPostalCodeBL(POSTNR_TXT, pageSize, pageNumber);
        }


        public List<CommunityDto> GetAllCommunities()
        {
            return businessLogic.GetAllCommunitiesBL();
        }


        public bool IsUserFollowingCommunity(int userId, int communityId)
        {
            return businessLogic.IsUserFollowingCommunityBL(userId, communityId);
        }


        public bool UserFollowCommunity(int userId, int communityId)
        {
            return businessLogic.UserFollowCommunityBL(userId, communityId);
        }


        public bool UserUnfollowCommunity(int userId, int communityId)
        {
            return businessLogic.UserUnfollowCommunityBL(userId, communityId);
        }

        #endregion


        #region ContactInfo Methods

        public bool AreUsersSharingContactInfo(int userAId, int userBId)
        {
            return businessLogic.AreUsersSharingContactInfoBL(userAId, userBId);
        }


        public bool IsContactInfoRequestPending(int userAId, int userBId)
        {
            return businessLogic.IsContactInfoRequestPendingBL(userAId, userBId);
        }


        public bool RequestContactInformation(int fromUserId, int toUserId)
        {
            return businessLogic.RequestContactInformationBL(fromUserId, toUserId);
        }


        public List<ContactInfoRequestDto> GetContactInfoRequestsToUser(int userId, int pageSize, int pageNumber)
        {
            return businessLogic.GetContactInfoRequestsToUserBL(userId, pageSize, pageNumber);
        }


        public int GetNumberOfContactInfoRequestsToUser(int userId)
        {
            return businessLogic.GetNumberOfContactInfoRequestsToUserBL(userId);
        }


        public bool AcceptContactInfoRequest(int contactInfoRequestId, int fromUserId, int toUserId)
        {
            return businessLogic.AcceptContactInfoRequestBL(contactInfoRequestId, fromUserId, toUserId);
        }


        public bool RejectContactInfoRequest(int contactInfoRequestId)
        {
            return businessLogic.RejectContactInfoRequestBL(contactInfoRequestId);
        }


        public bool StopSharingContactInfo(int userAId, int userBId)
        {
            return businessLogic.StopSharingContactInfoBL(userAId, userBId);
        }


        public List<UserDto> GetUsersWhoAreSharingContactInfoWithUser(int userId, int pageSize, int pageNumber)
        {
            return businessLogic.GetUsersWhoAreSharingContactInfoWithUserBL(userId, pageSize, pageNumber);
        }

        #endregion


        #region GeoNavigationMenu Methods

        public List<RegionDto> GetAllRegions()
        {
            return businessLogic.GetAllRegionsBL();
        }


        public List<MunicipalityDto> GetMunicipalitiesForRegion(string region)
        {
            return businessLogic.GetMunicipalitiesForRegionBL(region);
        }


        public List<PostalCodeDto> GetPostalCodesForMunicipality(string municipality)
        {
            return businessLogic.GetPostalCodesForMunicipalityBL(municipality);
        }


        public List<PostalCodeDto> GetAllPostalCodes()
        {
            return businessLogic.GetAllPostalCodesBL();
        }


        public List<MunicipalityDto> GetAllMunicipalities()
        {
            return businessLogic.GetAllMunicipalitiesBL();
        }


        public RegionDto GetUrlRegionName(string REGIONNAVN)
        {
            return businessLogic.GetUrlRegionNameBL(REGIONNAVN);
        }


        public MunicipalityDto GetUrlMunicipalityName(string KOMNAVN)
        {
            return businessLogic.GetUrlMunicipalityNameBL(KOMNAVN);
        }

        #endregion


        #region Media Methods

        /// <summary>
        /// Used by the worker role to update the URI for the resized photos (Large, Medium, Thumbnail).
        /// </summary>
        public bool UpdateResizedPhotoBlobUris(int mediaId, string blobUriLargePhoto, string blobUriMediumPhoto, string blobUriThumbnailPhoto)
        {
            return businessLogic.UpdateResizedPhotoBlobUrisBL(mediaId, blobUriLargePhoto, blobUriMediumPhoto, blobUriThumbnailPhoto);
        }


        /// <summary>
        /// Used by worker role to update the video blob URI to the converted video blob URI.
        /// </summary>
        public bool UpdateConvertedVideoBlobUri(int mediaId, string blobUriConvertedVideo)
        {
            return businessLogic.UpdateConvertedVideoBlobUriBL(mediaId, blobUriConvertedVideo);
        }


        public bool UpdatePhotoCaption(int mediaId, string photoCaptionText)
        {
            return businessLogic.UpdatePhotoCaptionBL(mediaId, photoCaptionText);
        }


        public bool UpdateVideoTitle(int mediaId, string videoTitleText)
        {
            return businessLogic.UpdateVideoTitleBL(mediaId, videoTitleText);
        }


        public bool DeleteMedia(int mediaId, BusinessLogic.MediaUsage mediaUsage)
        {
            return businessLogic.DeleteMediaBL(mediaId, mediaUsage);
        }


        /// <summary>
        /// Generic method which is the entry point to saving all media in the system.
        /// </summary>
        /// <param name="hostItemId">
        /// Media is hosted in conjunction with other items: Community, NewsItem, User, Assignment. The hostItemId is a generic term which describes the item id
        /// which connects to the media: CommunityID, NewsItemID, UserID, AssigmentID. In the method SaveMediaBL() scope, the hostItemId is not an unique identifier,
        /// but the SaveMediaBL() method will call other methods SaveCommunityMediaBL(), SaveNewsItemMediaBL(), SaveUserMedia(), SaveAssignmentMedia() and in those
        /// contexts the hostItemId will be a unique identifier in its relative contexts, namely as respectively: CommunityID, NewsItemID, UserID, AssigmentID.
        /// </param>
        /// <param name="blobUri"></param>
        /// <param name="mediaUsage"></param>
        /// <returns></returns>
        public bool SaveMedia(int hostItemId, string blobUri, BusinessLogic.MediaUsage mediaUsage)
        {
            return businessLogic.SaveMediaBL(hostItemId, blobUri, mediaUsage);
        }


        public bool SaveMediaFromPhone(int newsItemId, byte[] photo)
        {
            return businessLogic.SaveMediaFromPhoneBL(newsItemId, photo);
        }

        #endregion


        #region Message Methods

        public bool SendMessage(int receiverUserId, int senderUserId, string subject, string messageBody)
        {
            return businessLogic.SendMessageBL(receiverUserId, senderUserId, subject, messageBody);
        }


        public List<MessageDto> GetInboxContent(int userId, int pageSize, int pageNumber)
        {
            return businessLogic.GetInboxContentBL(userId, pageSize, pageNumber);
        }


        public List<MessageDto> GetOutboxContent(int userId, int pageSize, int pageNumber)
        {
            return businessLogic.GetOutboxContentBL(userId, pageSize, pageNumber);
        }


        public MessageDto GetMessage(string partitionKey, string rowKey)
        {
            return businessLogic.GetMessageBL(partitionKey, rowKey);
        }


        public bool DeleteMessage(BusinessLogic.MessageOwner messageOwner, string partitionKey, string rowKey)
        {
            return businessLogic.DeleteMessageBL(messageOwner, partitionKey, rowKey);
        }


        public int GetNumberOfUnreadMessages(int receiverUserId)
        {
            return businessLogic.GetNumberOfUnreadMessagesBL(receiverUserId);
        }


        public bool MarkMessageAsRead(string partitionKey, string rowKey)
        {
            return businessLogic.MarkMessageAsReadBL(partitionKey, rowKey);
        }

        #endregion


        #region NewsItem Methods

        public int CreateNewsItem(NewsItemDto newsItemDto)
        {
            return businessLogic.CreateNewsItemBL(newsItemDto);
        }


        public NewsItemDto GetNewsItem(int newsItemId)
        {
            return businessLogic.GetNewsItemBL(newsItemId);
        }


        public bool UpdateNewsItem(NewsItemDto newsItemDto)
        {
            return businessLogic.UpdateNewsItemBL(newsItemDto);
        }


        public bool DeleteNewsItem(int newsItemId)
        {
            return businessLogic.DeleteNewsItemBL(newsItemId);
        }


        public List<NewsItemDto> GetNewestNewsItemsFromDkCountry(int pageSize, int pageNumber)
        {
            return businessLogic.GetNewestNewsItemsFromDkCountryBL(pageSize, pageNumber);
        }


        public List<NewsItemDto> GetNewestNewsItemsFromRegion(string urlRegionName, int pageSize, int pageNumber)
        {
            return businessLogic.GetNewestNewsItemsFromRegionBL(urlRegionName, pageSize, pageNumber);
        }


        public List<NewsItemDto> GetNewestNewsItemsFromMunicipality(string urlMunicipalityName, int pageSize, int pageNumber)
        {
            return businessLogic.GetNewestNewsItemsFromMunicipalityBL(urlMunicipalityName, pageSize, pageNumber);
        }


        public List<NewsItemDto> GetNewestNewsItemsFromPostalCode(string POSTNR_TXT, int pageSize, int pageNumber)
        {
            return businessLogic.GetNewestNewsItemsFromPostalCodeBL(POSTNR_TXT, pageSize, pageNumber);
        }


        public List<NewsItemDto> GetNewsStreamForUser(int userId, int daysToGoBack, int pageSize, int pageNumber)
        {
            return businessLogic.GetNewsStreamForUserBL(userId, daysToGoBack, pageSize, pageNumber);
        }


        public List<NewsItemDto> GetNewsItemsCreatedByUser(int userId, int pageSize, int pageNumber)
        {
            return businessLogic.GetNewsItemsCreatedByUserBL(userId, pageSize, pageNumber);
        }


        public bool IncrementNumberOfSharesOfNewsItem(int newsItemId)
        {
            return businessLogic.IncrementNumberOfSharesOfNewsItemBL(newsItemId);
        }


        public NewsItemDto GetBreakingNewsFromCommunity(int communityId, int hoursToGoBack)
        {
            return businessLogic.GetBreakingNewsFromCommunityBL(communityId, hoursToGoBack);
        }


        public NewsItemDto PollingForLatestBreakingNewsFromCommunity(int currentBreakingNewsItemId, int communityId, int hoursToGoBack)
        {
            return businessLogic.PollingForLatestBreakingNewsFromCommunityBL(currentBreakingNewsItemId, communityId, hoursToGoBack);
        }


        public NewsItemDto GetBreakingNewsFromPostalCode(string POSTNR_TXT, int hoursToGoBack)
        {
            return businessLogic.GetBreakingNewsFromPostalCodeBL(POSTNR_TXT, hoursToGoBack);
        }


        public NewsItemDto PollingForLatestBreakingNewsFromPostalCode(int currentBreakingNewsItemId, string POSTNR_TXT, int hoursToGoBack)
        {
            return businessLogic.PollingForLatestBreakingNewsFromPostalCodeBL(currentBreakingNewsItemId, POSTNR_TXT, hoursToGoBack);
        }


        public NewsItemDto GetBreakingNewsFromNewsStream(int userId, int hoursToGoBack)
        {
            return businessLogic.GetBreakingNewsFromNewsStreamBL(userId, hoursToGoBack);
        }


        public NewsItemDto PollingForLatestBreakingNewsFromNewsStream(int currentBreakingNewsItemId, int userId, int hoursToGoBack)
        {
            return businessLogic.PollingForLatestBreakingNewsFromNewsStreamBL(currentBreakingNewsItemId, userId, hoursToGoBack);
        }


        public NewsItemDto GetTrendingNewsFromDkCountry(int hoursToGoBack)
        {
            return businessLogic.GetTrendingNewsFromDkCountryBL(hoursToGoBack);
        }


        public NewsItemDto GetTrendingNewsFromRegion(string urlRegionName, int hoursToGoBack)
        {
            return businessLogic.GetTrendingNewsFromRegionBL(urlRegionName, hoursToGoBack);
        }


        public NewsItemDto GetTrendingNewsFromMunicipality(string urlMunicipalityName, int hoursToGoBack)
        {
            return businessLogic.GetTrendingNewsFromMunicipalityBL(urlMunicipalityName, hoursToGoBack);
        }

        #endregion


        #region NewsItemCategory Methods

        public List<NewsItemCategoryDto> GetNewsItemCategories()
        {
            return businessLogic.GetNewsItemCategoriesBL();
        }


        public List<NewsItemCategoryIndexDto> GetNewsItemCategoriesIndex()
        {
            return businessLogic.GetNewsItemCategoriesIndexBL();
        }


        public NewsItemCategoryIndexDto GetNewsItemCategory(int categoryId)
        {
            return businessLogic.GetNewsItemCategoryBL(categoryId);
        }


        public List<NewsItemDto> GetNewestNewsItemsInCategory(int categoryId, int pageSize, int pageNumber)
        {
            return businessLogic.GetNewestNewsItemsInCategoryBL(categoryId, pageSize, pageNumber);
        }

        #endregion


        #region Notification Methods

        public UserAlertDto GetUserAlerts(int userId)
        {
            return businessLogic.GetUserAlertsBL(userId);
        }


        public bool UpdateUserAlerts(UserAlertDto dto)
        {
            return businessLogic.UpdateUserAlertsBL(dto);
        }

        #endregion


        #region Poll Methods

        public PollDto GetPoll(int pollId)
        {
            return businessLogic.GetPollBL(pollId);
        }


        public int CreatePoll(string question, string areaIdentifier, string uiAreaIdentifier, int addedByUserId, int pollTypeId)
        {
            return businessLogic.CreatePollBL(question, areaIdentifier, uiAreaIdentifier, addedByUserId, pollTypeId);
        }


        public bool UpdatePoll(int pollId, string questionText, bool isCurrent, bool isArchived)
        {
            return businessLogic.UpdatePollBL(pollId, questionText, isCurrent, isArchived);
        }


        public bool DeletePoll(int pollId)
        {
            return businessLogic.DeletePollBL(pollId);
        }


        public bool SetPollCurrent(int pollId, string areaIdentifier)
        {
            return businessLogic.SetPollCurrentBL(pollId, areaIdentifier);
        }


        public bool SetPollArchived(int pollId)
        {
            return businessLogic.SetPollArchivedBL(pollId);
        }


        public PollDto GetCurrentCommunityPoll(int communityId, int userId)
        {
            return businessLogic.GetCurrentCommunityPollBL(communityId, userId);
        }


        public PollDto GetCurrentAnonymousPoll(string areaIdentifier)
        {
            return businessLogic.GetCurrentAnonymousPollBL(areaIdentifier);
        }


        public PollDto VoteCommunityPoll(int pollOptionId, int userId)
        {
            return businessLogic.VoteCommunityPollBL(pollOptionId, userId);
        }


        public PollDto VoteAnonymousPoll(int pollOptionId)
        {
            return businessLogic.VoteAnonymousPollBL(pollOptionId);
        }


        public int CreatePollOption(int pollId, int addedByUserId, string optionText)
        {
            return businessLogic.CreatePollOptionBL(pollId, addedByUserId, optionText);
        }


        public bool UpdatePollOption(int pollOptionId, string optionText)
        {
            return businessLogic.UpdatePollOptionBL(pollOptionId, optionText);
        }


        public bool DeletePollOption(int pollOptionId)
        {
            return businessLogic.DeletePollOptionBL(pollOptionId);
        }


        public List<PollOptionDto> GetPollOptions(int pollId)
        {
            return businessLogic.GetPollOptionsBL(pollId);
        }


        public List<PollDto> GetPollsForCommunity(int communityId, int userId, bool isArchived, int pageSize, int pageNumber)
        {
            return businessLogic.GetPollsForCommunityBL(communityId, userId, isArchived, pageSize, pageNumber);
        }


        public List<PollDto> GetPollsForArea(string areaIdentifier, bool isArchived, int pageSize, int pageNumber)
        {
            return businessLogic.GetPollsForAreaBL(areaIdentifier, isArchived, pageSize, pageNumber);
        }


        public List<AnonymousPollTypeDto> GetAnonymousPollTypes()
        {
            return businessLogic.GetAnonymousPollTypesBL();
        }

        #endregion


        #region PushNotification Methods

        public bool RegisterPushNotificationChannel(int userId, string channel)
        {
            return businessLogic.RegisterPushNotificationChannelBL(userId, channel);
        }

        #endregion


        #region Search Methods

        public List<NewsItemDto> GetNewsItemsClosestToPosition(double searchCenterLatitude, double searchCenterLongitude, int daysToGoBack, int pageSize, int pageNumber)
        {
            return businessLogic.GetNewsItemsClosestToPositionBL(searchCenterLatitude, searchCenterLongitude, daysToGoBack, pageSize, pageNumber);
        }


        public List<CommunityDto> GetCommunitiesClosestToPosition(double searchCenterLatitude, double searchCenterLongitude, int pageSize, int pageNumber)
        {
            return businessLogic.GetCommunitiesClosestToPositionBL(searchCenterLatitude, searchCenterLongitude, pageSize, pageNumber);
        }


        public List<UserDto> GetUsersClosestToPosition(double searchCenterLatitude, double searchCenterLongitude, int pageSize, int pageNumber)
        {
            return businessLogic.GetUsersClosestToPositionBL(searchCenterLatitude, searchCenterLongitude, pageSize, pageNumber);
        }


        public List<NewsItemDto> SearchNewsItems(SearchNewsItemDto dto)
        {
            return businessLogic.SearchNewsItemsBL(dto);
        }


        public List<CommunityDto> SearchCommunities(SearchCommunityDto dto)
        {
            return businessLogic.SearchCommunitiesBL(dto);
        }


        public List<UserDto> SearchUsers(SearchUserDto dto)
        {
            return businessLogic.SearchUsersBL(dto);
        }

        #endregion


        #region SpatialQuery Methods

        public bool IsLatLongWithinDenmark(double latitude, double longitude)
        {
            return businessLogic.IsLatLongWithinDenmarkBL(latitude, longitude);
        }

        public List<PolygonDto> GetDKCountryPolygons()
        {
            return businessLogic.GetDKCountryPolygonsBL();
        }


        public List<PolygonDto> GetRegionPolygons(string region)
        {
            return businessLogic.GetRegionPolygonsBL(region);
        }


        public List<PolygonDto> GetRegionPolygonsWithoutHoles(string region)
        {
            return businessLogic.GetRegionPolygonsWithoutHolesBL(region);
        }


        public List<PolygonDto> GetMunicipalityPolygons(string municipality)
        {
            return businessLogic.GetMunicipalityPolygonsBL(municipality);
        }


        public List<PolygonDto> GetMunicipalityPolygonsWithoutHoles(string municipality)
        {
            return businessLogic.GetMunicipalityPolygonsWithoutHolesBL(municipality);
        }


        public List<PolygonDto> GetPostalCodePolygons(string postalCode)
        {
            return businessLogic.GetPostalCodePolygonsBL(postalCode);
        }


        public bool IsGeographyInDenmark(string geographyWkt)
        {
            return businessLogic.IsGeographyInDenmarkBL(geographyWkt);
        }


        public bool IsPolygonValid(string geographyWkt)
        {
            return businessLogic.IsPolygonValidBL(geographyWkt);
        }


        public bool IsPolygonTooBig(string geographyWkt)
        {
            return businessLogic.IsPolygonTooBigBL(geographyWkt);
        }


        public bool IsUserLivingInCommunityArea(int userId, int communityId)
        {
            return businessLogic.IsUserLivingInCommunityAreaBL(userId, communityId);
        }


        public PointDto GetCenterPointOfDkCountry()
        {
            return businessLogic.GetCenterPointOfDkCountryBL();
        }


        public PointDto GetCenterPointOfRegion(string urlRegionName)
        {
            return businessLogic.GetCenterPointOfRegionBL(urlRegionName);
        }


        public PointDto GetCenterPointOfMunicipality(string urlMunicipalityName)
        {
            return businessLogic.GetCenterPointOfMunicipalityBL(urlMunicipalityName);
        }


        public PointDto GetCenterPointOfPostalCode(string POSTNR_TXT)
        {
            return businessLogic.GetCenterPointOfPostalCodeBL(POSTNR_TXT);
        }


        public PointDto GetCenterPointOfCommunity(int communityId)
        {
            return businessLogic.GetCenterPointOfCommunityBL(communityId);
        }

        #endregion


        #region User Methods

        public UserDto GetUser(int userId)
        {
            return businessLogic.GetUserBL(userId);
        }


        public int CreateUser(UserDto userDto)
        {
            return businessLogic.CreateUserBL(userDto);
        }


        public bool UpdateUser(UserDto userDto)
        {
            return businessLogic.UpdateUserBL(userDto);
        }


        public bool DeleteUser(int userId)
        {
            return businessLogic.DeleteUserBL(userId);
        }


        public int UserLogin(string email, string password)
        {
            return businessLogic.UserLoginBL(email, password);
        }


        public bool IsUserBlocked(string email, string password)
        {
            return businessLogic.IsUserBlockedBL(email, password);
        }


        public string[] GetRolesForUser(string userEmail)
        {
            return businessLogic.GetRolesForUserBL(userEmail);
        }


        public bool UpdateLastLoginPosition(int userId, double latitude, double longitude)
        {
            return businessLogic.UpdateLastLoginPositionBL(userId, latitude, longitude);
        }


        public bool IsEmailInUse(string email)
        {
            return businessLogic.IsEmailInUseBL(email);
        }


        public string GetUserEmail(int userId)
        {
            return businessLogic.GetUserEmailBL(userId);
        }


        public bool ChangePassword(string email, string oldPassword, string newPassword)
        {
            return businessLogic.ChangePasswordBL(email, oldPassword, newPassword);
        }


        public List<UserDto> GetLatestActiveUsersFromDkCountry(int pageSize, int pageNumber)
        {
            return businessLogic.GetLatestActiveUsersFromDkCountryBL(pageSize, pageNumber);
        }


        public List<UserDto> GetLatestActiveUsersFromRegion(string urlRegionName, int pageSize, int pageNumber)
        {
            return businessLogic.GetLatestActiveUsersFromRegionBL(urlRegionName, pageSize, pageNumber);
        }


        public List<UserDto> GetLatestActiveUsersFromMunicipality(string urlMunicipalityName, int pageSize, int pageNumber)
        {
            return businessLogic.GetLatestActiveUsersFromMunicipalityBL(urlMunicipalityName, pageSize, pageNumber);
        }


        public List<UserDto> GetLatestActiveUsersFromPostalCode(string POSTNR_TXT, int pageSize, int pageNumber)
        {
            return businessLogic.GetLatestActiveUsersFromPostalCodeBL(POSTNR_TXT, pageSize, pageNumber);
        }


        public List<UserDto> GetAllUsers()
        {
            return businessLogic.GetAllUsersBL();
        }


        public int GetNumberOfRequestsAndUnreadMessagesToUser(int userId)
        {
            return businessLogic.GetNumberOfRequestsAndUnreadMessagesToUserBL(userId);
        }

        #endregion
    }
}
