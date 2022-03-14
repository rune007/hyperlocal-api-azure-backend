using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class PolygonDto
    {
        [DataMember]
        public string PolygonWkt { get; set; }
    }
}