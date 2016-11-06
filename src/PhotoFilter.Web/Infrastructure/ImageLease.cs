using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoFilter.Web.Infrastructure
{
    public class ImageLease
    {
        public async Task<string> TryAcquireLeaseAsync(CloudBlockBlob blob, TimeSpan leasePeriod)
        {
            try
            {
                var id = await blob.AcquireLeaseAsync(leasePeriod, null);
                return id;
            }
            catch (StorageException exception)
            {
                if (exception.IsConflictLeaseAlreadyPresent())
                {
                    return null;
                }
                else if (exception.IsNotFoundBlobOrContainerNotFound())
                {
                    // If someone deleted the receipt, there's no lease to acquire.
                    return null;
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
