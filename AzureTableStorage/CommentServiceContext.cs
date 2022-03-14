using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
//using System.Web;
//using Microsoft.WindowsAzure;
//using Microsoft.WindowsAzure.StorageClient;

namespace HLServiceRole.AzureTableStorage
{
    public class CommentServiceContext
: Microsoft.WindowsAzure.StorageClient.TableServiceContext
    {
        public CommentServiceContext(string baseAddress, Microsoft.WindowsAzure.StorageCredentials credentials)
            : base(baseAddress, credentials)
        { }

        public IQueryable<CommentEntityModel> HlTable
        {
            get
            {
                return this.CreateQuery<CommentEntityModel>("HlTable");
            }
        }
    }
}