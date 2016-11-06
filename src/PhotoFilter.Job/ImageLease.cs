using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace PhotoFilter.Job
{
    public static class ImageLease
    {
        public static async Task TryReleaseLeaseAsync(CloudBlockBlob blob, string leaseId)
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
