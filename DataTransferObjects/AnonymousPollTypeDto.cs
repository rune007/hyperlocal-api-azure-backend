using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    /// <summary>
    /// This is Used when the Users in the Editor Role can create an anonymous Poll, there are 4 different 
    /// types of anonymous Polls: Country, Region, Municipality and Postal Code.
    /// </summary>
    [DataContract]
    public class AnonymousPollTypeDto
    {
        [DataMember]
        public int AnonymousPollTypeID { get; set; }

        [DataMember]
        public string Name { get; set; }
    }
}