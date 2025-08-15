using Serilog;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.IO;

namespace Shared.Utilities
{
    public class SerilogHelper
    {
        public static void Configure()
        {
            var batched = new WebApiBatchedSink("https://ggapi.movsar.dev/Logs");
            var batchingWrapper = new PeriodicBatchingSink(
                batched,
                new PeriodicBatchingSinkOptions
                {
                    BatchSizeLimit = 50,
                    Period = TimeSpan.FromSeconds(5),
                    EagerlyEmitFirstEvent = true
                });

            string logPath = Path.Combine("logs", "logs.txt");
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("OSVersion", Environment.OSVersion.VersionString)
                .WriteTo.Sink(batchingWrapper)
                .WriteTo.File(logPath, rollingInterval: RollingInterval.Day)
                .WriteTo.File(Path.Combine("logs","logs.txt"), rollingInterval: RollingInterval.Day, shared: true) // optional
                .CreateLogger();
        }
    }
}
