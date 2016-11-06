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

        public ImageStore(CloudQueueClient queueClient, CloudBlobContainer container)
        {
            _container = container;
            _queueClient = queueClient;
        }

        public async Task<FetchContract> FetchAsync(int count, BlobContinuationToken continuationToken = null)
        {
            var blobs = await _container.ListBlobsSegmentedAsync(
                prefix: string.Empty,
                useFlatBlobListing: true,
                blobListingDetails: BlobListingDetails.All,
                maxResults: 20,
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
                var blockBlob = (CloudBlockBlob)x;
                if (numAquiredImages < count && blockBlob.Properties.LeaseState != LeaseState.Leased)
                {
                    try
                    {
                        //await blockBlob.AcquireLeaseAsync(TimeSpan.FromSeconds(10));
                        //await blockBlob.ReleaseLeaseAsync(AccessCondition.GenerateLeaseCondition("photolease"));
                        //await blockBlob.StartCopyAsync()
                        numAquiredImages++;

                        return new Image
                        {
                            Id = x.Uri.ToString(),
                        };
                    }
                    catch (Exception exception)
                    {

                    }
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
