using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class NewsItemCategoryDto
    {
        [DataMember]
        public int CategoryID { get; set; }

        [DataMember]
        public string CategoryName { get; set; }
    }
}