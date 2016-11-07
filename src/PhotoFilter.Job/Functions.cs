using System.IO;
using Microsoft.Azure.WebJobs;
using System;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;

namespace PhotoFilter.Job
{
    public class Functions
    {
        public static async Task ProcessIncoming(
            [QueueTrigger("incoming")] Image image,
            [Blob("allphotos/{BlobName}", FileAccess.Read)] Stream blobInputStream,
            [Blob("allphotos/{BlobName}")] CloudBlockBlob blobInputBlob,
            [Blob("confirmedphotos/{BlobName}", FileAccess.Write)] Stream blobPhotoOutput,
            [Blob("notphotos/{BlobName}", FileAccess.Write)] Stream blobNonPhotoOutput)
        {
            if(blobInputStream == null)
            {
                Console.WriteLine("NULL");
                return;
            }

            Console.WriteLine($"IsPhoto: {image.IsPhoto}");
            if (image.IsPhoto)
                await blobInputStream.CopyToAsync(blobPhotoOutput);
            else
                await blobInputStream.CopyToAsync(blobNonPhotoOutput);

            await ImageLease.TryReleaseLeaseAsync(blobInputBlob, image.LeaseId);
            blobInputBlob.DeleteIfExists();
        }
    }
}
