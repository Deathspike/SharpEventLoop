using System;
using System.IO;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace SharpEventLoop.Examples.Listener
{
    public class Program
    {
        private static async Task RunRequestAsync(HttpListenerContext context)
        {
            try
            {
                var response = context.Response;

                using (var streamWriter = new StreamWriter(response.OutputStream))
                {
                    Console.WriteLine("[{0}] Sending the greeting ...", Thread.CurrentThread.ManagedThreadId);

                    await streamWriter.WriteAsync("Hello world!");

                    Console.WriteLine("[{0}] Finished the greeting!", Thread.CurrentThread.ManagedThreadId);
                }
            }
            catch (HttpListenerException)
            {
                Console.WriteLine("[{0}] Rats! The connection went away :-(", Thread.CurrentThread.ManagedThreadId);
            }
        }

        private static async Task RunListenerAsync()
        {
            var httpListener = new HttpListener();
            httpListener.Prefixes.Add("http://localhost:3000/");
            httpListener.Start();

            while (true)
            {
                Console.WriteLine("[{0}] Waiting for a request ...", Thread.CurrentThread.ManagedThreadId);

                var context = await httpListener.GetContextAsync();
                EventLoop.Run(() => RunRequestAsync(context));
                if (context.Request.Url.LocalPath == "/quit") break;

                Console.WriteLine("[{0}] Enqueued! Waiting for the next request!", Thread.CurrentThread.ManagedThreadId);
            }
        }

        public static void Main(string[] args)
        {
            Console.WriteLine("Listening on http://localhost:3000/");
            Console.WriteLine("Hit http://localhost:3000/quit to break the listener!");
            EventLoop.Pump(() => EventLoop.Run(RunListenerAsync));
        }
    }
}