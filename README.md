# SharpEventLoop

SharpEventLoop is an NodeJS-inspired asynchronous event loop for .NET 4.5/Mono 2.12 and higher.

## Installation

    Install-Package SharpEventLoop

## Introduction

*NodeJS* is essentially an elaborate event loop with a JavaScript runtime. Tasks involving I/O run in the background using OS-specific non-blocking mechanisms and upon completion of the task a continuation is scheduled to run on the event loop. It scales pretty well and ensures that the code a developer writes is effectively lock/synchronization free for all intents and purposes. Asynchronous variants of existing I/O methods were added in *.NET 4.5*, and in combination with the introduced `async` and `await` keywords it became trivial to develop asynchronous code and have the same asynchronous efficiency as *NodeJS*. This project adds an event loop implementation similar to that of *NodeJS* and enables a similar lock/synchronization-free development experience.

## Methods

The static `EventLoop` class resides in the `System` namespace and contains:

### EventLoop.Pump(initializer)

This method creates an event loop, invokes the initialization method and enters the event loop. The initializer allows for the initial events to be scheduled on the event loop. In comparison with *NodeJS*, the initializer  serves the same role as the application/root script. While the event loop is not considered to be empty, the thread entering the pump will be occupied with waiting or processing of events.

### EventLoop.Run(worker)

This method schedules a worker on the event loop. Upon invocation of the worker, the method runs until a `Task` is returned. The returned `Task` continuation is then scheduled at the end of the event loop. The *.NET* compiler transparently splits methods involving the `async` and `await` keywords into separate methods with `Task` continuations, so continuations are also scheduled on the event loop.

## Examples

### HTTP Requests

    public class Program {
        public static void Main() {
            EventLoop.Pump(() => {
                EventLoop.Run(() => Print("http://www.bing.com/"));
                EventLoop.Run(() => Print("http://www.google.com/"));
            });
        }

        public static async Task Print(string address) {
            using (var client = new WebClient()) {
                Console.WriteLine(await client.DownloadStringTaskAsync(address));
            }
        }
    }

### HTTP Server

    public class Program {
        public static void Main() {
            EventLoop.Pump(() => {
                Console.WriteLine("Running on http://localhost:3000/");
                EventLoop.Run(WebServer);
            });
        }

        public static async Task WebServer() {
            var listener = new HttpListener();
            listener.Prefixes.Add("http://localhost:3000/");
            listener.Start();
            while (true) {
                var context = await httpListener.GetContextAsync();
                EventLoop.Run(() => WebServerRequest(context));
            }
        }

        public static async Task WebServerRequest(HttpListenerContext context) {
            using (var writer = new StreamWriter(context.Response.OutputStream)) {
                await writer.WriteAsync("Hello world!");
            }
        }
    }

## Conclusion

An event loop is easily brought to *.NET 4.5*. 
