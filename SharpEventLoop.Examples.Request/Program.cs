using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SharpEventLoop.Examples.Request
{
    public class Program
    {
        private static async Task<string> DownloadGooglePageAsync()
        {
            using (var webClient = new WebClient())
            {
                return await webClient.DownloadStringTaskAsync(new Uri("http://www.google.com/"));
            }
        }

        private static async Task RepeatDownloadGooglePages(int n)
        {
            for (var i = 0; i < 5; i++)
            {
                Console.WriteLine("Task #{0} Loop #{1} Thread #{2} (Pre)", n, i, Thread.CurrentThread.ManagedThreadId);

                await DownloadGooglePageAsync();

                Console.WriteLine("Task #{0} Loop #{1} Thread #{2} (Post)", n, i, Thread.CurrentThread.ManagedThreadId);
            }
        }

        public static void Main()
        {
            EventLoop.Pump(() =>
            {
                for (var i = 0; i < 10; i++)
                {
                    var n = i;
                    EventLoop.Run(() => RepeatDownloadGooglePages(n));
                }
            });
        }
    }
}