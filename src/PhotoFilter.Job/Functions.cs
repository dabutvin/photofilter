using System.IO;
using Microsoft.Azure.WebJobs;
using System;

namespace PhotoFilter.Job
{
    public class Functions
    {
        public static void ProcessIncoming([QueueTrigger("incoming")] Image image, TextWriter log)
        {
            Console.WriteLine(image.Id);
            Console.WriteLine(image.IsPhoto);
            log.WriteLine(image.Id);
        }
    }
}
