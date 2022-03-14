using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class MunicipalityDto
    {
        [DataMember]
        public string KOMNAVN { get; set; }

        [DataMember]
        public string UrlMunicipalityName { get; set; }

        [DataMember]
        public string PolygonWkt { get; set; }
    }
}