using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.WindowsAzure.StorageClient;

namespace HLServiceRole.AzureTableStorage
{
    public class CommentEntityModel : TableServiceEntity
    {

        public CommentEntityModel()
        {
        }


        public CommentEntityModel(int newsItemId)
            // PartitionKey is newsItemId, but we are using a prefix of "0" to distinguish it from the partition key of Messages which we are storing in the 
            // same table.
            // RowKey is reversed DateTime.Now.Ticks, because we want the newest comments to be at beginning of the list.
            : base("0" + newsItemId.ToString(), string.Format("{0:10}_{1}", DateTime.MaxValue.Ticks - DateTime.Now.Ticks, Guid.NewGuid()))
        {
            CreateDate = DateTime.Now;
        }

        public int UserID { get; set; }
        public DateTime CreateDate { get; set; }
        public string CommentBody { get; set; }
    }
}