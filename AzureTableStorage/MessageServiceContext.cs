using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace HLServiceRole.AzureTableStorage
{
    public class MessageServiceContext
    : Microsoft.WindowsAzure.StorageClient.TableServiceContext
    {
        public MessageServiceContext(string baseAddress, Microsoft.WindowsAzure.StorageCredentials credentials)
            : base(baseAddress, credentials)
        { }

        public IQueryable<MessageEntityModel> HlTable
        {
            get
            {
                return this.CreateQuery<MessageEntityModel>("HlTable");
            }
        }
    }

}
