using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace SharpEventLoop.Examples.Listener
{
    public class Program
    {
        public static void Main()
        {
            EventLoop.Pump(() =>
            {
                Console.WriteLine("Running on http://localhost:3000/");
                EventLoop.Run(WebServer);
            });
        }

        public static async Task WebServer()
        {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:3000/");
            listener.Start();

            while (true)
            {
                var context = await listener.GetContextAsync();
                EventLoop.Run(() => WebServerRequest(context));
            }
        }

        public static async Task WebServerRequest(HttpListenerContext context)
        {
            using (var writer = new StreamWriter(context.Response.OutputStream))
            {
                await writer.WriteAsync("Hello world!");
            }
        }
    }
}