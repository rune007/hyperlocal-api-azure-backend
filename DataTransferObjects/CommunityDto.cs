using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class CommunityDto
    {
        [DataMember]
        public int CommunityID { get; set; }

        [DataMember]
        public int AddedByUserID { get; set; }

        [DataMember]
        public string AddedByUserFullName { get; set; }

        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public string PolygonWkt { get; set; }

        [DataMember]
        public DateTime CreateUpdateDate { get; set; }

        [DataMember]
        public string ImageBlobUri { get; set; }

        [DataMember]
        public string MediumSizeBlobUri { get; set; }

        [DataMember]
        public string ThumbnailBlobUri { get; set; }

        [DataMember]
        public bool HasPhoto { get; set; }

        [DataMember]
        public double PolygonCenterLatitude { get; set; }

        [DataMember]
        public double PolygonCenterLongitude { get; set; }

        [DataMember]
        public int NumberOfUsersInCommunity { get; set; }

        /// <summary>
        /// The time when there was last added a NewsItem within the area of the Community.
        /// </summary>
        [DataMember]
        public DateTime? LatestActivity { get; set; }

        /* Bing map had problem with DateTime so I convert to LatestActivityToString, which is a string version of LatestActivity. */
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