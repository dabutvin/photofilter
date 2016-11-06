using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoFilter.Web.Infrastructure
{
    public class ImageStore
    {
        private readonly CloudBlobContainer _container;
        private readonly CloudQueueClient _queueClient;
        private readonly ImageLease _imageLease;

        public ImageStore(CloudQueueClient queueClient, CloudBlobContainer container, ImageLease imageLease)
        {
            _container = container;
            _queueClient = queueClient;
            _imageLease = imageLease;
        }

        public async Task<FetchContract> FetchAsync(int count, BlobContinuationToken continuationToken = null)
        {
            var blobs = await _container.ListBlobsSegmentedAsync(
                prefix: string.Empty,
                useFlatBlobListing: true,
                blobListingDetails: BlobListingDetails.All,
                maxResults: 36,
                currentToken: continuationToken,
                options: new BlobRequestOptions
                {
                },
                operationContext: new OperationContext
                {
                }
            );

            var numAquiredImages = 0;

            var images = await blobs.Results.ForEachAsync(async x =>
            {
                try
                {
                    if (numAquiredImages < count)
                    {
                        var blockBlob = (CloudBlockBlob)x;
                        var leaseId = await _imageLease.TryAcquireLeaseAsync(blockBlob, TimeSpan.FromSeconds(30));

                        if (!string.IsNullOrEmpty(leaseId))
                        {
                            ImageLease.LeaseIdLookup[x.Uri.ToString()] = leaseId;
                            //await _imageLease.TryReleaseLeaseAsync(blockBlob, leaseId);
                            //await blockBlob.StartCopyAsync()
                            numAquiredImages++;

                            return new Image
                            {
                                Id = x.Uri.ToString(),
                            };
                        }
                    }
                }
                catch (Exception exception)
                {

                }

                return null;
            });

            return new FetchContract
            {
                Images = images.Where(x => x != null).ToArray(),
                ContinuationToken = blobs.ContinuationToken,
            };
        }

        public async Task<bool> Sort(Image[] images)
        {
            var queue = _queueClient.GetQueueReference("incoming");
            await queue.CreateIfNotExistsAsync();
            foreach (var image in images)
            {
                await queue.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(image)));
            }

            return true;
        }
    }
}
