using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class PostalCodeDto
    {
        [DataMember]
        public string POSTNR_TXT { get; set; }

        [DataMember]
        public string POSTBYNAVN { get; set; }

        [DataMember]
        public string PolygonWkt { get; set; }
    }
}