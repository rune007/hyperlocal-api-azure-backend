using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class RegionDto
    {
        [DataMember]
        public string REGIONNAVN { get; set; }

        [DataMember]
        public string UrlRegionName { get; set; }

        [DataMember]
        public string PolygonWkt { get; set; }
    }
}