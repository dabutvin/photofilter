using Microsoft.Azure.WebJobs;

namespace PhotoFilter.Job
{
    class Program
    {
        static void Main()
        {
            var host = new JobHost();
            host.RunAndBlock();
        }
    }
}
