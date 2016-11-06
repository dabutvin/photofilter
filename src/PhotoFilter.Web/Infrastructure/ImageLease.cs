using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace PhotoFilter.Web.Infrastructure
{
    public class ImageLease
    {
        public static Dictionary<string, string> LeaseIdLookup => new Dictionary<string, string>();

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

        public async Task TryReleaseLeaseAsync(CloudBlockBlob blob, string leaseId)
        {
            try
            {
                // Note that this call returns without throwing if the lease is expired. See the table at:
                // http://msdn.microsoft.com/en-us/library/azure/ee691972.aspx
                await blob.ReleaseLeaseAsync(
                    accessCondition: new AccessCondition { LeaseId = leaseId },
                    options: null,
                    operationContext: null);
            }
            catch (StorageException exception)
            {
                if (exception.IsNotFoundBlobOrContainerNotFound())
                {
                    // The user deleted the receipt or its container; nothing to release at this point.
                }
                else if (exception.IsConflictLeaseIdMismatchWithLeaseOperation())
                {
                    // Another lease is active; nothing for this lease to release at this point.
                }
                else
                {
                    throw;
                }
            }
        }
    }
}
