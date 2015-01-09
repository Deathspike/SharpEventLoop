# SharpEventLoop

SharpEventLoop is an NodeJS-inspired asynchronous event loop for .NET 4.5/Mono 2.12 and higher.

## Installation

    Install-Package SharpEventLoop

## Motivation

*NodeJS* has is designed to avoid headaches with thread and parallel processing. It is event-driven and non-blocking, and ensures that the code a developer writes is effectively lock/synchronization free (and single-threaded) for all intents and purposes. This is accomplished by doing IO-bound tasks in the background and scheduling each completion to run on the event loop. It scales well and is easy to use. This library aims to implement a similar event loop in C#.

## Usage

An event loop is initialized, which runs an *initializer* and blocks the calling thread. Example:

    EventLoop.Pump(eventLoop =>
    {
        // ... initialize ...
    });

As in *NodeJS*, the event loop will stop pumping and exit when it is empty. A more complex example:

    var result = new List<string>();

    EventLoop.Pump(eventLoop =>
    {
        for (var i = 0; i < 2; i++)
        {
            eventLoop.Run(async () =>
            {
                using (var wc = new WebClient())
                {
                    result.Add(await wc.DownloadStringTaskAsync("http://www.google.com/"));
                }
            });
        }
    });

    // ... Do something with the result.

`List<string>` is not thread safe, so the above is dangerous when implemented with `Tasks`.

## Final Word

By Roel van Uden; written as an experiment. Don't hesistate to ask questions! More examples in the solution!
