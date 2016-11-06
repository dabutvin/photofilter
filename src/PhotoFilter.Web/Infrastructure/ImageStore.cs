﻿using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
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

        private async Task<BlobResultSegment> ListBlobs(BlobContinuationToken continuationToken)
        {
            return await _container.ListBlobsSegmentedAsync(
                prefix: string.Empty,
                useFlatBlobListing: true,
                blobListingDetails: BlobListingDetails.All,
                maxResults: 48,
                currentToken: continuationToken,
                options: new BlobRequestOptions
                {
                },
                operationContext: new OperationContext
                {
                }
            );
        }

        private async Task<Image[]> MapBlobsToImages(IEnumerable<IListBlobItem> blobs, int maxNeeded)
        {
            var numAquiredImages = 0;
            return (await blobs.ForEachAsync(async x =>
            {
                try
                {
                    if (numAquiredImages < maxNeeded)
                    {
                        var blockBlob = (CloudBlockBlob)x;
                        var leaseId = await _imageLease.TryAcquireLeaseAsync(blockBlob, TimeSpan.FromSeconds(60));

                        if (!string.IsNullOrEmpty(leaseId))
                        {
                            numAquiredImages++;

                            return new Image
                            {
                                Id = x.Uri.ToString(),
                                BlobName = blockBlob.Name,
                                LeaseId = leaseId,
                            };
                        }
                    }
                }
                catch (Exception exception)
                {

                }

                return null;
            })).Where(x => x != null).ToArray();
        }

        public async Task<FetchContract> FetchAsync(int count, BlobContinuationToken continuationToken = null)
        {
            var blobs = await ListBlobs(continuationToken);
            var images = await MapBlobsToImages(blobs.Results, count);

            while (blobs.ContinuationToken != null && images.Length < count)
            {
                blobs = await ListBlobs(blobs.ContinuationToken);
                images = images.Concat(await MapBlobsToImages(blobs.Results, count - images.Length)).ToArray();
            }

            return new FetchContract
            {
                Images = images,
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
