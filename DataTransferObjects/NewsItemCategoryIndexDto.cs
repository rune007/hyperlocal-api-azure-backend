using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;

namespace HLServiceRole.DataTransferObjects
{
    // This DTO carries the information for showing an overview and status of the different NewsItemCategories.
    // Besides carrying information about the NewsItemCategories, it also carries the information of the 
    // latest NewsItem added in a particular NewsItemCategory. This information is used to give an overview
    // of all the NewsItemCategories in the view Category/Index.
    public class NewsItemCategoryIndexDto
    {

        [DataMember]
        public int CategoryID { get; set; }

        [DataMember]
        public string CategoryName { get; set; }

        [DataMember]
        public int NumberOfNewsItemsInCategory { get; set; }

        // The fields below carry the info of the latest NewItem in a particular NewItemCategory.
        // This information is used in the overview of the NewsItemCategories in the view
        // Category/Index

        [DataMember]
        public int? NewsItemID { get; set; }

        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string Story { get; set; }

        [DataMember]
        public string PositionPointWkt { get; set; }

        [DataMember]
        public double Latitude { get; set; }

        [DataMember]
        public double Longitude { get; set; }

        [DataMember]
        public DateTime CreateUpdateDate { get; set; }

        /* Bing map had problem with DateTime so I convert to CreateUpdateDateToString, which is a string version of CreateUpdateDate. */
        [DataMember]
        public string CreateUpdateDateToString { get; set; }

        [DataMember]
        public string ConverPhotoLarge { get; set; }

        [DataMember]
        public string CoverPhotoMediumSize { get; set; }

        [DataMember]
        public string CoverPhotoThumbNail { get; set; }

        [DataMember]
        public bool HasPhoto { get; set; }
    }
}