using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PhotoFilter.Web.Infrastructure
{
    public class ImageCount
    {
        private readonly CloudBlobContainer _allPhotosContainer;
        private readonly CloudBlobContainer _numPhotoContainer;
        private readonly CloudBlobContainer _numNonPhotosContainer;

        public ImageCount(CloudBlobClient blobClient)
        {
            _allPhotosContainer = blobClient.GetContainerReference("testallphotos");
            _allPhotosContainer.CreateIfNotExistsAsync();

            _numPhotoContainer = blobClient.GetContainerReference("testphoto");
            _numPhotoContainer.CreateIfNotExistsAsync();

            _numNonPhotosContainer = blobClient.GetContainerReference("testnonphoto");
            _numNonPhotosContainer.CreateIfNotExistsAsync();
        }

        public async Task<CountContract> CountAsync()
        {
            var nonPhotoBlobs = CountTotalBlobs(_numNonPhotosContainer);
            var photoBlobs = CountTotalBlobs(_numPhotoContainer);
            var totalBlobs = CountTotalBlobs(_allPhotosContainer);

            return new CountContract
            {
                NumNonPhoto = await nonPhotoBlobs,
                NumPhoto = await nonPhotoBlobs,
                NumTotal = await totalBlobs,
            };
        }

        private async Task<int> CountTotalBlobs(CloudBlobContainer container)
        {
            try
            {
                BlobContinuationToken token = null;
                var count = 0;
                do
                {
                    var result = await container.ListBlobsSegmentedAsync(token);
                    token = result.ContinuationToken;
                    count += result.Results.Count();
                } while (token == null);

                return count;
            }
            catch(Exception exception)
            {

            }

            return 0;

        }
    }
}
