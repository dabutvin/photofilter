using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
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
                maxResults: count,
                currentToken: continuationToken,
                options: new BlobRequestOptions
                {

                },
                operationContext: new OperationContext
                {

                }
            );

            return new FetchContract
            {
                Images = blobs.Results.Select(x => new Image
                {
                    Id = x.Uri.ToString(),
                }).ToArray(),
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
