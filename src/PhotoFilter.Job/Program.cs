using Microsoft.Azure.WebJobs;
using StackExchange.Redis;

namespace PhotoFilter.Job
{
    class Program
    {
        public static ConnectionMultiplexer Redis { get; set; }

        public static void Resetredis()
        {
            Redis = ConnectionMultiplexer.Connect("highscore.redis.cache.windows.net,abortConnect=false,ssl=true,password=bm74xhq66IbzLtCnkWj+34thhsV0pEIYsKDZStJje1E=");
        }

        static void Main()
        {
            try
            {
                Resetredis();
            }
            catch { }
            var host = new JobHost();
            host.RunAndBlock();
        }
    }
}
