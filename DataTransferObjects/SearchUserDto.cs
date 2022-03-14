using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    // This DTO is used mostly to carry the search parameters in relation to search on Users.
    [DataContract]
    public class SearchUserDto
    {
        [DataMember]
        public string FirstName { get; set; }

        [DataMember]
        public string LastName { get; set; }

        [DataMember]
        public string Bio { get; set; }

        [DataMember]
        public string Email { get; set; }

        [DataMember]
        public string Address { get; set; }

        [DataMember]
        public string Phone { get; set; }

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