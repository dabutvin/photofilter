using Microsoft.WindowsAzure.Storage.Table;

namespace PhotoFilter.Job
{
    public class Highscore : TableEntity
    {
        public int Score { get; set; }
    }
}
