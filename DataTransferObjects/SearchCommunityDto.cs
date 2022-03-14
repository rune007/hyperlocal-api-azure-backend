using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    // This DTO is used mostly to carry the search parameters in relation to search on Communities.
    [DataContract]
    public class SearchCommunityDto
    {
        [DataMember]
        public string Name { get; set; }

        [DataMember]
        public string Description { get; set; }

        [DataMember]
        public double SearchCenterLatitude { get; set; }

        [DataMember]
        public double SearchCenterLongitude { get; set; }

        [DataMember]
        public int? SearchRadius { get; set; }

        [DataMember]
        public int PageSize { get; set; }

        [DataMember]
        public int PageNumber { get; set; }
    }
}