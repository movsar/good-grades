using Data;
using Serilog;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;

namespace Shared.Utilities
{
    public class SerilogHelper
    {
        public static void Configure()
        {
            var batched = new WebApiBatchedSink(
                endpoint: "https://ggapi.movsar.dev/Logs",
                bufferDirectory: Path.Combine(AppContext.BaseDirectory, "logs", "outbox"),
                maxBufferBytes: 50 * 1024 * 1024
            );

            var batchingWrapper = new PeriodicBatchingSink(
                batched,
                new PeriodicBatchingSinkOptions
                {
                    BatchSizeLimit = 50,
                    Period = TimeSpan.FromSeconds(5),
                    EagerlyEmitFirstEvent = true
                });

            Directory.CreateDirectory(Path.Combine("logs"));
            string localLogsPath = Path.Combine("logs", "logs.txt");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .Enrich.FromLogContext()
                .Enrich.WithProperty("MachineName", Environment.MachineName)
                .Enrich.WithProperty("OSVersion", Environment.OSVersion.VersionString)
                .WriteTo.Sink(batchingWrapper)
                .WriteTo.File(localLogsPath, rollingInterval: RollingInterval.Day, shared: true)
                .CreateLogger();
        }

        #region Deprecated
        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);
        private List<LogMessage> ParseLogs(string content)
        {
            // Define compiled regex patterns for better performance
            Regex TimestampRegex = new(@"^(.*?)\s*(?=\[)", RegexOptions.Compiled);
            Regex TagRegex = new(@"(?<=\[).+?(?=\])", RegexOptions.Compiled);
            Regex MessageRegex = new(@"(?<=\]).*", RegexOptions.Compiled);

            var logs = new List<LogMessage>();
            //добавлено /z к выражению, чтобы не пропускался последний лог из-за отсутсвия новой даты или новой строки
            var regex = new Regex(@"^\d{4}-\d{2}-\d{2}.*?(?=^\d{4}-\d{2}-\d{2}|\z)",
                RegexOptions.Multiline | RegexOptions.Singleline);

            foreach (Match match in regex.Matches(content))
            {
                var logEntry = match.Value.Trim();
                if (string.IsNullOrWhiteSpace(logEntry))
                {
                    continue;
                }

                var lines = logEntry.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                if (lines.Length == 0)
                {
                    continue;
                }

                // Parse log details
                var firstLine = lines[0];

                DateTime.TryParse(TimestampRegex.Match(firstLine).Value, out var createdAt);
                var tag = TagRegex.Match(firstLine).Value;
                var message = MessageRegex.Match(firstLine).Value.Trim();
                var stackTrace = lines.Length > 1 ? string.Join(Environment.NewLine, lines.Skip(1)) : null;

                long memKb;
                GetPhysicallyInstalledSystemMemory(out memKb);
                var installedRamInGb = memKb / 1024 / 1024;

                logs.Add(new LogMessage
                {
                    Id = 0,
                    Message = message,
                    StackTrace = stackTrace,
                    CreatedAt = createdAt,
                    Level = tag switch
                    {
                        "ERR" => (int)LogLevel.Error,
                        "WRN" => (int)LogLevel.Warning,
                        "INF" => (int)LogLevel.Information,
                        _ => (int)LogLevel.Debug
                    },
                    ProgramName = Assembly.GetEntryAssembly()?.GetName().Name,
                    ProgramVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
                    WindowsVersion = Environment.OSVersion.VersionString,
                    SystemDetails = $"{installedRamInGb}Gb | {(Environment.Is64BitOperatingSystem ? "64bit" : "32bit")} | {Environment.MachineName}"
                });

            }

            return logs;
        }
        #endregion
    }
}