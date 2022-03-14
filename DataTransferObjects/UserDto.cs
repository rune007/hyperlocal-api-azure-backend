using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class UserDto
    {
        [DataMember]
        public int UserID { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Password { get; set; }

        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string FullName { get; set; }

        [DataMember]
        public string Bio { get; set; }

        [DataMember]
        public string PhoneNumber { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string AddressPositionPointWkt { get; set; }

        [DataMember]
        public string ImageBlobUri { get; set; }

        [DataMember]
        public string MediumSizeBlobUri { get; set; }

        [DataMember]
        public string ThumbnailBlobUri { get; set; }

        [DataMember]
        public bool HasPhoto { get; set; }

        [DataMember]
        public DateTime CreateDate { get; set; }

        [DataMember]
        public DateTime ProfileLastUpdatedDate { get; set; }

        [DataMember]
        public int RoleID { get; set; }

        [DataMember]
        public bool Blocked { get; set; }

        [DataMember]
        public string PushNotificationChannel { get; set; }

        [DataMember]
        public string LastLoginPositionPointWkt { get; set; }

        [DataMember]
        public DateTime LastLoginDateTime { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public double LastLoginLatitude { get; set; }

        [DataMember]
        public double LastLoginLongitude { get; set; }

        [DataMember]
        public int NumberOfNewsItemsPostedByUser { get; set; }

        [DataMember]
        public DateTime? LatestActivity { get; set; }

        /* Bing map had problem with DateTime so I convert to LatestActivityToString, which is a string version of LatestActivity. 
         LatestActivity is the last time a User posted a NewsItem. */
        [DataMember]
        public string LatestActivityToString { get; set; }

        /// <summary>
        /// This is used with paging, and indicates whether there is more data to fetch when paging through data with server side data paging.
        /// </summary>
        [DataMember]
        public bool HasNextPageOfData { get; set; }

        /// <summary>
        /// In conjunction with search this is used to carry the number of search results which matches the search.
        /// </summary>
        [DataMember]
        public int NumberOfSearchResults { get; set; }
    }
}