using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoFilter.Web.Infrastructure
{
    public class FetchContract
    {
        public BlobContinuationToken ContinuationToken { get; set; }
        public Image[] Images { get; set; }
    }
}
