using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class AssignmentDto
    {
        [DataMember]
        public int AssignmentID { get; set; }

        [DataMember]
        public int AddedByUserID { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public DateTime CreateUpdateDate { get; set; }

        [DataMember]
        public DateTime ExpiryDate { get; set; }

        [DataMember]
        public string ImageBlobUri { get; set; }

        [DataMember]
        public string MediumSizeBlobUri { get; set; }

        [DataMember]
        public string ThumbnailBlobUri { get; set; }

        [DataMember]
        public bool HasPhoto { get; set; }

        [DataMember]
        public int NumberOfNewsItemsOnAssignment { get; set; }

        /// <summary>
        /// The latest time a NewsItem was added on an Assignment.
        /// </summary>
        [DataMember]
        public DateTime? LatestActivity { get; set; }

        [DataMember]
        public string LatestActivityToString { get; set; }

        /// <summary>
        /// The two fields below transfers the position where 
        /// there were latest added News on the Assignment.
        /// </summary>
        [DataMember]
        public double? LatestNewsLatitude { get; set; }

        [DataMember]
        public double? LatestNewsLongitude { get; set; }

        /// <summary>
        /// This is used with paging, and indicates whether there is more data to fetch when paging through data with server side data paging.
        /// </summary>
        [DataMember]
        public bool HasNextPageOfData { get; set; }

        /// <summary>
        /// True is the ExpiryDate has been passed.
        /// </summary>
        [DataMember]
        public bool IsExpired { get; set; }


        #region GeoTemporalAssignment fields

        [DataMember]
        public double AssignmentCenterLatitude { get; set; }

        [DataMember]
        public double AssignmentCenterLongitude { get; set; }

        [DataMember]
        public string AssignmentCenterAddress { get; set; }

        [DataMember]
        public int? AssignmentRadius { get; set; }

        [DataMember]
        public string AreaPolygonWkt { get; set; }

        /// <summary>
        /// Number of hours to go back when looking for Users who have 
        /// logged in within the Assignment area.
        /// </summary>
        [DataMember]
        public int HoursToGoBack { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int PageNumber { get; set; }

        #endregion
    }
}