using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class PollOptionDto
    {
        [DataMember]
        public int PollOptionID { get; set; }

        [DataMember]
        public int PollID { get; set; }

        [DataMember]
        public int AddedByUserID { get; set; }

        [DataMember]
        public DateTime CreateUpdateDate { get; set; }

        [DataMember]
        public string OptionText { get; set; }

        [DataMember]
        public int Votes { get; set; }
    }
}