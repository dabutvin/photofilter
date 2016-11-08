using System.IO;
using Microsoft.Azure.WebJobs;
using System;
using Microsoft.WindowsAzure.Storage.Blob;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.WindowsAzure.Storage.Table;

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
            if (blobInputStream == null)
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

            try
            {
                var highscoredb = Program.Redis.GetDatabase();
                await highscoredb.HashIncrementAsync("scores", image.Email);
            }
            catch
            {
                Program.Resetredis();
                throw;
            }
        }

        [Singleton]
        public static async Task ProcessBackup(
            [TimerTrigger("00:01:00", RunOnStartup =true)] TimerInfo timerInfo,
            [Table("highscore")] CloudTable tableBinding)
        {
            var highscoredb = Program.Redis.GetDatabase();
            var allRedisScores = await highscoredb.HashGetAllAsync("scores");

            var highScoresFromRedis = allRedisScores.Select(x => new Highscore
            {
                PartitionKey = "partition1",
                RowKey = x.Name,
                Score = int.Parse(x.Value),
            });

            foreach(var highScoreFromRedis in highScoresFromRedis)
            {
                var existingScoreFromTable = tableBinding
                    .CreateQuery<Highscore>()
                    .AsQueryable()
                    .Where(x => x.RowKey == highScoreFromRedis.RowKey)
                    .Take(1)
                    .FirstOrDefault();

                if (existingScoreFromTable != null)
                {
                    if (existingScoreFromTable.Score < highScoreFromRedis.Score)
                    {
                        existingScoreFromTable.Score = highScoreFromRedis.Score;
                        tableBinding.Execute(TableOperation.Replace(existingScoreFromTable));
                    }
                }
                else
                {
                    var insertOperation = TableOperation.Insert(highScoreFromRedis);
                    tableBinding.Execute(insertOperation);
                }
            }
        }
    }
}
