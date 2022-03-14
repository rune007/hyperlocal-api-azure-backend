using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    [DataContract]
    public class PollDto
    {
        [DataMember]
        public int PollID { get; set; }

        [DataMember]
        public int AddedByUserID { get; set; }

        /// <summary>
        /// The system supports 5 types of Polls: 
        /// 1. Country, 2. Region, 3. Municipality, 4. Postal Code, 5. Community
        /// </summary>
        [DataMember]
        public int PollTypeID { get; set; }

        [DataMember]
        public DateTime CreateUpdateDate { get; set; }

        /// <summary>
        /// E.g. : "2700", "Hilleroed", "Sjaelland", "Country".
        /// </summary>
        [DataMember]
        public string AreaIdentifier { get; set; }

        /// <summary>
        /// E.g. : "2700 Brønshøj", "Hillerød", "Sjælland", "Danmark".
        /// </summary>
        [DataMember]
        public string UiAreaIdentifier { get; set; }

        [DataMember]
        public string QuestionText { get; set; }

        [DataMember]
        public bool IsCurrent { get; set; }

        [DataMember]
        public bool IsArchived { get; set; }

        [DataMember]
        public DateTime ArchivedDate { get; set; }

        [DataMember]
        public bool HasUserVoted { get; set; }

        [DataMember]
        public List<PollOptionDto> PollOptions { get; set; }

        /// <summary>
        /// This is used with paging, and indicates whether there is more data to fetch when paging through data with server side data paging.
        /// </summary>
        [DataMember]
        public bool HasNextPageOfData { get; set; }
    }
}