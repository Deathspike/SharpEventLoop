using System;
using System.Net;
using System.Threading.Tasks;

namespace SharpEventLoop.Examples.Request
{
    public class Program
    {
        public static void Main()
        {
            EventLoop.Pump(() =>
            {
                EventLoop.Run(() => Print("http://www.bing.com/"));
                EventLoop.Run(() => Print("http://www.google.com/"));
            });
        }

        public static async Task Print(string address)
        {
            using (var client = new WebClient())
            {
                Console.WriteLine(await client.DownloadStringTaskAsync(address));
            }
        }
    }
}