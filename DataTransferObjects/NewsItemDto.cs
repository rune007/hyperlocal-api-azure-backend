using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class NewsItemDto
    {
        [DataMember]
        public int NewsItemID { get; set; }

        [DataMember]
        public int PostedByUserID { get; set; }

        [DataMember]
        public string PostedByUserName { get; set; }

        [DataMember]
        public int CategoryID { get; set; }

        [DataMember]
        public string CategoryName { get; set; }

        [DataMember]
        public int? AssignmentID { get; set; }

        [DataMember]
        public string AssignmentTitle { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Story { get; set; }

        [DataMember]
        public string PositionPointWkt { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public DateTime CreateUpdateDate { get; set; }

        /* Bing map had problem with DateTime so I convert to CreateUpdateDateToString, which is a string version of CreateUpdateDate. */
        [DataMember]
        public string CreateUpdateDateToString { get; set; }

        [DataMember]
        public bool IsLocalBreakingNews { get; set; }

        [DataMember]
        public int NumberOfViews { get; set; }

        [DataMember]
        public int NumberOfComments { get; set; }

        [DataMember]
        public int NumberOfShares { get; set; }

        [DataMember]
        public bool HasPhoto { get; set; }

        [DataMember]
        public bool HasVideo { get; set; }

        [DataMember]
        public string CoverPhotoLarge { get; set; }

        [DataMember]
        public string CoverPhotoMediumSize { get; set; }

        [DataMember]
        public string CoverPhotoThumbNail { get; set; }

        [DataMember]
        public List<NewsItemPhotoDto> Photos { get; set; }

        [DataMember]
        public List<NewsItemVideoDto> Videos { get; set; }

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

        /// <summary>
        /// The Community in which area a particular NewsItems happens to be located. This is determined with a spatial query.
        /// A NewsItem can be located within several Communities areas. If this is the case then which Community is selected is
        /// determined in the query.
        /// </summary>
        [DataMember]
        public int LocatedInCommunityID { get; set; }

        [DataMember]
        public string LocatedInCommunityName { get; set; }

        [DataMember]
        public int NumberOfNewsItemsCreatedByUser { get; set; }
    }
}