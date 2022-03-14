using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using HLServiceRole.DataTransferObjects;
using HLServiceRole.BusinessLogicLayer;

namespace HLServiceRole
{
    /// <summary>
    /// The methods are gathered in regions which are ordered alphabetical.
    /// </summary>
    [ServiceContract]
    public interface IHLService
    {
        #region Assignment Methods

        [OperationContract]
        AssignmentDto GetAssignment(int assignmentId);

        [OperationContract]
        int CreateAssignment(AssignmentDto assignmentDto);

        [OperationContract]
        bool UpdateAssignment(AssignmentDto assignmentDto);

        [OperationContract]
        bool DeleteAssignment(int assignmentId);

        /// <summary>
        /// A GeoTemporalAssignment inherits all the characteristics of an Assignment, the particular thing about it is that it alerts
        /// Users who have been within the AssignmentArea or live within the AssignmentArea about the GeoTemporalAssigment.
        /// (E.g. There is a traffic jam at the high way. A GeoTemporalAssignment is posted, alerting all the Users who have logged in to 
        /// hyperlocal.dk within the area (we track the login location of Users) within the last 2 hours + the Users who live next to 
        /// the place on the highway, of course only alerting Users who have chosen this kind of alerts in their alert settings.) 
        /// </summary>
        [OperationContract]
        int CreateGeoTemporalAssignment(AssignmentDto dto);

        [OperationContract]
        List<AssignmentDto> GetAssignmentsForDropDownList();

        [OperationContract]
        List<NewsItemDto> GetNewestNewsItemsOnAssignment(int assignmentId, int pageSize, int pageNumber);

        [OperationContract]
        List<AssignmentDto> GetActiveAssignments(int pageSize, int pageNumber);

        [OperationContract]
        List<AssignmentDto> GetAssignmentsCreatedByUser(int userId, int pageSize, int pageNumber);

        /// <summary>
        /// Gets Users within the area (AssignmentRadius), and time (HoursToGoBack) of a geo temporal Assignment.
        /// </summary>
        [OperationContract]
        List<UserDto> GetUsersWithinAreaAndTimeOfGeoTemporalAssignment(AssignmentDto dto);

        #endregion


        #region AzureBlob Methods

        /// <summary>
        /// Gets a SAS URI to write to a blob for 15 minutes.
        /// </summary>
        /// <param name="mediaUsage">Media is used with different objects: News, Users, Assignments or Communities.</param>
        [OperationContract]
        string GetSasUriForBlobWrite(BusinessLogic.MediaUsage mediaUsage, string fileName);

        /// <summary>
        /// Returns a SAS URI for read to a blob which expires after 10 hours.
        /// </summary>
        [OperationContract]
        string GetSasUriForBlobRead(string blobUri);


        [OperationContract]
        bool SaveImage(int salesItemId, string contentType, byte[] photo);

        #endregion


        #region Comment Methods

        [OperationContract]
        CommentDto CreateComment(int newsItemId, int userId, string commentBody);

        [OperationContract]
        bool DeleteComment(int newsItemId, string rowKey);

        [OperationContract]
        List<CommentDto> GetCommentsOnNewsItem(int newsItemId, int pageSize, int pageNumber);

        #endregion


        #region Community Methods

        [OperationContract]
        CommunityDto GetCommunity(int communityId);

        [OperationContract]
        int CreateCommunity(CommunityDto communityDto);

        [OperationContract]
        bool UpdateCommunity(CommunityDto communityDto);

        [OperationContract]
        bool DeleteCommunity(int communityId);

        [OperationContract]
        List<CommunityDto> GetCommunitiesCreatedByUser(int userId, int pageSize, int pageNumber);

        [OperationContract]
        List<CommunityDto> GetCommunitiesFollowedByUser(int userId, int pageSize, int pageNumber);

        [OperationContract]
        bool IsCommunityCreatedByUser(int userId, int communityId);

        [OperationContract]
        List<NewsItemDto> GetNewestNewsItemsFromCommunity(int communityId, int pageSize, int pageNumber);

        [OperationContract]
        List<UserDto> GetLatestActiveUsersFromCommunity(int communityId, int pageSize, int pageNumber);

        /// <summary>
        /// Returns a paginated list of Communities from the whole of Denmark territory. The Communities with the latest activity
        /// (Having had added NewsItems within their area latest) are in the beginning of the list.
        /// </summary>
        [OperationContract]
        List<CommunityDto> GetLatestActiveCommunitiesFromDkCountry(int pageSize, int pageNumber);

        [OperationContract]
        List<CommunityDto> GetLatestActiveCommunitiesFromRegion(string urlRegionName, int pageSize, int pageNumber);

        [OperationContract]
        List<CommunityDto> GetLatestActiveCommunitiesFromMunicipality(string urlMunicipalityName, int pageSize, int pageNumber);

        [OperationContract]
        List<CommunityDto> GetLatestActiveCommunitiesFromPostalCode(string POSTNR_TXT, int pageSize, int pageNumber);

        [OperationContract]
        List<CommunityDto> GetAllCommunities();

        [OperationContract]
        bool IsUserFollowingCommunity(int userId, int communityId);

        /// <summary>
        /// Makes a User follow a Communtiy.
        /// </summary>
        [OperationContract]
        bool UserFollowCommunity(int userId, int communityId);

        /// <summary>
        /// Makes a User unfollow a Communtiy.
        /// </summary>
        [OperationContract]
        bool UserUnfollowCommunity(int userId, int communityId);

        #endregion


        #region ContactInfo Methods

        [OperationContract]
        bool AreUsersSharingContactInfo(int userAId, int userBId);

        [OperationContract]
        bool IsContactInfoRequestPending(int userAId, int userBId);

        [OperationContract]
        bool RequestContactInformation(int fromUserId, int toUserId);

        [OperationContract]
        List<ContactInfoRequestDto> GetContactInfoRequestsToUser(int userId, int pageSize, int pageNumber);

        [OperationContract]
        int GetNumberOfContactInfoRequestsToUser(int userId);

        [OperationContract]
        int GetNumberOfRequestsAndUnreadMessagesToUser(int userId);

        [OperationContract]
        bool AcceptContactInfoRequest(int contactInfoRequestId, int fromUserId, int toUserId);

        [OperationContract]
        bool RejectContactInfoRequest(int contactInfoRequestId);

        /// <summary>
        /// Makes two Users stop sharing contact information.
        /// </summary>
        [OperationContract]
        bool StopSharingContactInfo(int userAId, int userBId);

        [OperationContract]
        List<UserDto> GetUsersWhoAreSharingContactInfoWithUser(int userId, int pageSize, int pageNumber);

        #endregion


        #region GeoNavigationMenu Methods

        /// <summary>
        /// Getting all the regions, used for navigation.
        /// </summary>
        [OperationContract]
        List<RegionDto> GetAllRegions();

        /// <summary>
        /// Getting the municipalities for a region, used for navigation.
        /// </summary>
        [OperationContract]
        List<MunicipalityDto> GetMunicipalitiesForRegion(string region);

        /// <summary>
        /// Getting the postal codes for a municipality, used for navigation.
        /// </summary>
        [OperationContract]
        List<PostalCodeDto> GetPostalCodesForMunicipality(string municipality);

        [OperationContract]
        List<PostalCodeDto> GetAllPostalCodes();

        [OperationContract]
        List<MunicipalityDto> GetAllMunicipalities();

        /// <summary>
        /// Takes in REGIONNAVN (e.g. "Sjælland") and returns UrlRegionName (e.g. "Sjaelland")
        /// </summary>
        /// <param name="REGIONNAVN">e.g. "Sjælland"</param>
        /// <returns>UrlRegionName e.g. "Sjaelland"</returns>
        [OperationContract]
        RegionDto GetUrlRegionName(string REGIONNAVN);

        /// <summary>
        /// Takes in KOMNAVN (e.g. "Hillerød") and returns UrlMunicipalityName (e.g. "Hilleroed")
        /// </summary>
        /// <param name="KOMNAVN">e.g. Hillerød</param>
        /// <returns>e.g. Hilleroed</returns>
        [OperationContract]
        MunicipalityDto GetUrlMunicipalityName(string KOMNAVN);

        #endregion


        #region Media Methods

        [OperationContract]
        bool UpdateResizedPhotoBlobUris(int mediaId, string blobUriLargePhoto, string blobUriMediumPhoto, string blobUriThumbnailPhoto);

        [OperationContract]
        bool UpdateConvertedVideoBlobUri(int mediaId, string blobUriConvertedVideo);

        [OperationContract]
        bool UpdatePhotoCaption(int mediaId, string photoCaptionText);

        [OperationContract]
        bool UpdateVideoTitle(int mediaId, string videoTitleText);

        [OperationContract]
        bool DeleteMedia(int mediaId, BusinessLogic.MediaUsage mediaUsage);

        [OperationContract]
        bool SaveMedia(int hostItemId, string blobUri, BusinessLogic.MediaUsage mediaUsage);

        [OperationContract]
        bool SaveMediaFromPhone(int newsItemId, byte[] photo);

        #endregion


        #region Message Methods

        [OperationContract]
        bool SendMessage(int receiverUserId, int senderUserId, string subject, string messageBody);

        [OperationContract]
        List<MessageDto> GetInboxContent(int userId, int pageSize, int pageNumber);

        [OperationContract]
        List<MessageDto> GetOutboxContent(int userId, int pageSize, int pageNumber);

        [OperationContract]
        MessageDto GetMessage(string partitionKey, string rowKey);

        [OperationContract]
        bool DeleteMessage(BusinessLogic.MessageOwner messageOwner, string partitionKey, string rowKey);

        [OperationContract]
        int GetNumberOfUnreadMessages(int receiverUserId);

        [OperationContract]
        bool MarkMessageAsRead(string partitionKey, string rowKey);

        #endregion


        #region NewsItem Methods

        [OperationContract]
        NewsItemDto GetNewsItem(int newsItemId);

        /// <returns>The ID of the newly created NewsItem</returns>
        [OperationContract]
        int CreateNewsItem(NewsItemDto newsItemDto);

        [OperationContract]
        bool UpdateNewsItem(NewsItemDto newsItemDto);

        [OperationContract]
        bool DeleteNewsItem(int newsItemId);

        [OperationContract]
        List<NewsItemDto> GetNewestNewsItemsFromDkCountry(int pageSize, int pageNumber);

        [OperationContract]
        List<NewsItemDto> GetNewestNewsItemsFromRegion(string urlRegionName, int pageSize, int pageNumber);

        [OperationContract]
        List<NewsItemDto> GetNewestNewsItemsFromMunicipality(string urlMunicipalityName, int pageSize, int pageNumber);

        [OperationContract]
        List<NewsItemDto> GetNewestNewsItemsFromPostalCode(string POSTNR_TXT, int pageSize, int pageNumber);

        [OperationContract]
        List<NewsItemDto> GetNewsStreamForUser(int userId, int daysToGoBack, int pageSize, int pageNumber);

        [OperationContract]
        List<NewsItemDto> GetNewsItemsCreatedByUser(int userId, int pageSize, int pageNumber);

        /// <summary>
        /// Increments the NumberOfShares, the number of times the NewsItem has been shared on social media.
        /// </summary>
        [OperationContract]
        bool IncrementNumberOfSharesOfNewsItem(int newsItemId);

        /// <summary>
        /// Gets the latest breaking NewsItem (Marked as IsLocalBreakingNews) from within the area of a
        /// particular Community and which is not older than hoursToGoBack subtracted from the current time.
        /// </summary>
        [OperationContract]
        NewsItemDto GetBreakingNewsFromCommunity(int communityId, int hoursToGoBack);

        /// <summary>
        /// Compares the current breaking news from the server with the one we display in the client. If it's the same we just return null.
        /// But if it's not the same, we return this latest breaking news to the client.
        /// </summary>
        [OperationContract]
        NewsItemDto PollingForLatestBreakingNewsFromCommunity(int currentBreakingNewsItemId, int communityId, int hoursToGoBack);

        [OperationContract]
        NewsItemDto GetBreakingNewsFromPostalCode(string POSTNR_TXT, int hoursToGoBack);

        [OperationContract]
        NewsItemDto PollingForLatestBreakingNewsFromPostalCode(int currentBreakingNewsItemId, string POSTNR_TXT, int hoursToGoBack);

        [OperationContract]
        NewsItemDto GetBreakingNewsFromNewsStream(int userId, int hoursToGoBack);

        [OperationContract]
        NewsItemDto PollingForLatestBreakingNewsFromNewsStream(int currentBreakingNewsItemId, int userId, int hoursToGoBack);

        /// <summary>
        /// Gets the NewsItem which have generated the most User interaction (in the way of
        /// NumberOfComments and NumberOfShares - social media sharings) from within the area 
        /// of a Denmark, in the time frame defined by hoursToGoBack
        /// </summary>
        [OperationContract]
        NewsItemDto GetTrendingNewsFromDkCountry(int hoursToGoBack);

        [OperationContract]
        NewsItemDto GetTrendingNewsFromRegion(string urlRegionName, int hoursToGoBack);

        /// <summary>
        /// Gets the NewsItem which have generated the most User interaction (in the way of
        /// NumberOfComments and NumberOfShares - social media sharings) from within the area 
        /// of a Municipality, in the time frame defined by hoursToGoBack
        /// </summary>
        [OperationContract]
        NewsItemDto GetTrendingNewsFromMunicipality(string urlMunicipalityName, int hoursToGoBack);

        #endregion


        #region NewsItemCategory Methods

        [OperationContract]
        List<NewsItemCategoryDto> GetNewsItemCategories();

        /// <summary>
        /// This method returns data used for giving an overview of the different NewsItemCategories. Besides data about
        /// each NewsItemCategory it also yields data about the latest NewsItem added in each NewsItemCategory.
        /// </summary>
        [OperationContract]
        List<NewsItemCategoryIndexDto> GetNewsItemCategoriesIndex();

        [OperationContract]
        NewsItemCategoryIndexDto GetNewsItemCategory(int categoryId);

        [OperationContract]
        List<NewsItemDto> GetNewestNewsItemsInCategory(int categoryId, int pageSize, int pageNumber);

        #endregion


        #region Notification Methods

        /// <summary>
        /// UserAlerts are the settings which determines which notifications the Users receive from the system and how they receive them.
        /// That is AlertOnBreakingNews, AlertOnMessages, SendEmail, UsePushNotification, etc.
        /// </summary>
        [OperationContract]
        UserAlertDto GetUserAlerts(int userId);

        [OperationContract]
        bool UpdateUserAlerts(UserAlertDto dto);

        #endregion


        #region Poll Methods

        [OperationContract]
        PollDto GetPoll(int pollId);

        [OperationContract]
        int CreatePoll(string question, string areaIdentifier, string uiAreaIdentifier, int addedByUserId, int pollTypeId);

        [OperationContract]
        bool UpdatePoll(int pollId, string questionText, bool isCurrent, bool isArchived);

        [OperationContract]
        bool DeletePoll(int pollId);

        [OperationContract]
        bool SetPollCurrent(int pollId, string areaIdentifier);

        [OperationContract]
        bool SetPollArchived(int pollId);

        [OperationContract]
        PollDto GetCurrentCommunityPoll(int communityId, int userId);

        [OperationContract]
        PollDto GetCurrentAnonymousPoll(string areaIdentifier);

        [OperationContract]
        PollDto VoteCommunityPoll(int pollOptionId, int userId);

        [OperationContract]
        PollDto VoteAnonymousPoll(int pollOptionId);

        [OperationContract]
        int CreatePollOption(int pollId, int addedByUserId, string optionText);

        [OperationContract]
        bool UpdatePollOption(int pollOptionId, string optionText);

        [OperationContract]
        bool DeletePollOption(int pollOptionId);

        [OperationContract]
        List<PollOptionDto> GetPollOptions(int pollId);

        [OperationContract]
        List<PollDto> GetPollsForCommunity(int communityId, int userId, bool isArchived, int pageSize, int pageNumber);

        /// <summary>
        /// Gets anonymous Polls for different kind of areas: e.g. "Country", "Sjaelland", "Hilleroed", "2700"
        /// </summary>
        /// <param name="areaIdentifier">a string which uniquely identifies an area, e.g. "Country" (The whole of Denmark)
        /// ""Sjaelland" (Region), "Hilleroed" (Municipality), "2700" (PostalCode)</param>
        [OperationContract]
        List<PollDto> GetPollsForArea(string areaIdentifier, bool isArchived, int pageSize, int pageNumber);

        [OperationContract]
        List<AnonymousPollTypeDto> GetAnonymousPollTypes();

        #endregion


        #region PushNotification Methods

        /// <summary>
        /// Register Push Notification Channel URI coming in from a particular application in a particular Windows Phone 7 device.
        /// </summary>
        [OperationContract]
        bool RegisterPushNotificationChannel(int userId, string channel);

        #endregion


        #region Search Methods

        /// <summary>
        /// Returns NewsItems in paginable list ordered according to how close they are to the search center.
        /// The variable daysToGoBack determines how many days we will go back in our NewsItems from the current date.
        /// </summary>
        /// <param name="daysToGoBack">How many daysToGoBack = How old can the NewsItems be</param>
        [OperationContract]
        List<NewsItemDto> GetNewsItemsClosestToPosition(double searchCenterLatitude, double searchCenterLongitude, int daysToGoBack, int pageSize, int pageNumber);

        [OperationContract]
        List<CommunityDto> GetCommunitiesClosestToPosition(double searchCenterLatitude, double searchCenterLongitude, int pageSize, int pageNumber);

        [OperationContract]
        List<UserDto> GetUsersClosestToPosition(double searchCenterLatitude, double searchCenterLongitude, int pageSize, int pageNumber);

        [OperationContract]
        List<NewsItemDto> SearchNewsItems(SearchNewsItemDto dto);

        [OperationContract]
        List<CommunityDto> SearchCommunities(SearchCommunityDto dto);

        [OperationContract]
        List<UserDto> SearchUsers(SearchUserDto dto);

        #endregion


        #region SpatialQuery Methods

        [OperationContract]
        bool IsLatLongWithinDenmark(double latitude, double longitude);

        /// <summary>
        /// Getting the polygons covering the whole of Danish territory.
        /// </summary>
        [OperationContract]
        List<PolygonDto> GetDKCountryPolygons();

        /// <summary>
        /// Getting the polygons for a particular region.
        /// </summary>
        [OperationContract]
        List<PolygonDto> GetRegionPolygons(string region);

        /// <summary>
        /// Gets the polygons for a particular region, but without holes in the polygons. This is for rendering on 
        /// the Windows Phone. My C# parsing the WKT, on the phone, does not understand holes in polygons.
        /// </summary>
        [OperationContract]
        List<PolygonDto> GetRegionPolygonsWithoutHoles(string region);

        /// <summary>
        /// Getting the polygons for a particular municipality.
        /// </summary>
        [OperationContract]
        List<PolygonDto> GetMunicipalityPolygons(string municipality);

        /// <summary>
        /// Gets the polygons for a particular municipality, but without holes in the polygons. This is for rendering on 
        /// the Windows Phone. My C# parsing the WKT, on the phone, does not understand holes in polygons.
        /// </summary>
        [OperationContract]
        List<PolygonDto> GetMunicipalityPolygonsWithoutHoles(string municipality);

        /// <summary>
        /// Getting the polygons for a particular postal code.
        /// </summary>
        [OperationContract]
        List<PolygonDto> GetPostalCodePolygons(string postalCode);

        /// <summary>
        /// Checks whether a geography is in Danish territory.
        /// </summary>
        [OperationContract]
        bool IsGeographyInDenmark(string geographyWkt);

        /// <summary>
        /// Checks whether a geography WKT can be made into a valid polygon.
        /// </summary>
        [OperationContract]
        bool IsPolygonValid(string geographyWkt);

        /// <summary>
        /// Checks whether a Community exceeds the size limit of 15 SQ KM.
        /// </summary>
        [OperationContract]
        bool IsPolygonTooBig(string geographyWkt);

        /// <summary>
        /// Checks whether a particular User lives inside the area of a particular Community.
        /// </summary>
        [OperationContract]
        bool IsUserLivingInCommunityArea(int userId, int communityId);

        /// <summary>
        /// Gets the center point of Denmark, used for adjusting map.
        /// </summary>
        [OperationContract]
        PointDto GetCenterPointOfDkCountry();

        /// <summary>
        /// Gets the center point of a Region, used for adjusting map.
        /// </summary>
        [OperationContract]
        PointDto GetCenterPointOfRegion(string urlRegionName);

        /// <summary>
        /// Gets the center point of a Municipality, used for adjusting map.
        /// </summary>
        [OperationContract]
        PointDto GetCenterPointOfMunicipality(string urlMunicipalityName);

        /// <summary>
        /// Gets the center point of a PostalCode, used for adjusting map.
        /// </summary>
        [OperationContract]
        PointDto GetCenterPointOfPostalCode(string POSTNR_TXT);

        /// <summary>
        /// Gets the center point of a Community, used for adjusting map.
        /// </summary>
        [OperationContract]
        PointDto GetCenterPointOfCommunity(int communityId);

        #endregion


        #region User Methods

        [OperationContract]
        UserDto GetUser(int userId);

        /// <summary>
        /// Creates a new User in the system and returns the UserID of the new User.
        /// </summary>
        [OperationContract]
        int CreateUser(UserDto userDto);

        [OperationContract]
        bool UpdateUser(UserDto userDto);

        [OperationContract]
        bool DeleteUser(int userId);

        /// <summary>
        /// Returns the UserID if a Users credentials are valid.
        /// </summary>
        [OperationContract]
        int UserLogin(string email, string password);

        /// <summary>
        /// Checks whether a user has been blocked.
        /// </summary>
        [OperationContract]
        bool IsUserBlocked(string email, string password);

        [OperationContract]
        string[] GetRolesForUser(string userEmail);

        [OperationContract]
        bool UpdateLastLoginPosition(int userId, double latitude, double longitude);

        /// <summary>
        /// Checking whether the email is already in the system.
        /// </summary>
        [OperationContract]
        bool IsEmailInUse(string email);

        [OperationContract]
        string GetUserEmail(int userId);

        [OperationContract]
        bool ChangePassword(string email, string oldPassword, string newPassword);

        /// <summary>
        /// Return a paginated list of Users from the whole of Denmark territory. The Users who have 
        /// latest uploaded a NewsItem (LatestActive) are in the beginning of the list.
        /// </summary>
        [OperationContract]
        List<UserDto> GetLatestActiveUsersFromDkCountry(int pageSize, int pageNumber);

        [OperationContract]
        List<UserDto> GetLatestActiveUsersFromRegion(string urlRegionName, int pageSize, int pageNumber);

        [OperationContract]
        List<UserDto> GetLatestActiveUsersFromMunicipality(string urlMunicipalitynName, int pageSize, int pageNumber);

        [OperationContract]
        List<UserDto> GetLatestActiveUsersFromPostalCode(string POSTNR_TXT, int pageSize, int pageNumber);

        [OperationContract]
        List<UserDto> GetAllUsers();

        #endregion
    }
}
