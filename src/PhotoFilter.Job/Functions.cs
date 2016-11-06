using System.IO;
using Microsoft.Azure.WebJobs;
using System;
using Microsoft.WindowsAzure.Storage.Blob;

namespace PhotoFilter.Job
{
    public class Functions
    {
        public static void ProcessIncoming(
            [QueueTrigger("incoming")] Image image,
            [Blob("testallphotos/{BlobName}", FileAccess.Read)] Stream blobInputStream,
            [Blob("testallphotos/{BlobName}")] CloudBlockBlob blobInputBlob,
            [Blob("testphoto/{BlobName}", FileAccess.Write)] Stream blobPhotoOutput,
            [Blob("testnonphoto/{BlobName}", FileAccess.Write)] Stream blobNonPhotoOutput)
        {
            if(blobInputStream == null)
            {
                Console.WriteLine("NULL");
                return;
            }

            Console.WriteLine($"IsPhoto: {image.IsPhoto}");
            if (image.IsPhoto)
                blobInputStream.CopyTo(blobPhotoOutput);
            else
                blobInputStream.CopyTo(blobNonPhotoOutput);

            // pass the leaseID and release the lease
            blobInputBlob.DeleteIfExists();
        }
    }
}
