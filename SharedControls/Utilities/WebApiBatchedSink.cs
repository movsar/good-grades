using Data;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Shared.Utilities
{
    public sealed class WebApiBatchedSink : IBatchedLogEventSink, IDisposable
    {
        private readonly HttpClient _httpClient;
        private readonly Uri _endpoint;
        private readonly string? _programName;
        private readonly string? _programVersion;
        private readonly string _windowsVersion;
        private readonly string _systemDetails;

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        };

        [DllImport("kernel32.dll")]
        private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryKb);

        public WebApiBatchedSink(string endpoint)
        {
            _endpoint = new Uri(endpoint);
            _httpClient = new HttpClient();

            var entry = System.Reflection.Assembly.GetEntryAssembly();
            _programName = entry?.GetName().Name;
            _programVersion = entry?.GetName().Version?.ToString();
            _windowsVersion = Environment.OSVersion.VersionString;

            long memKb = 0;
            try { GetPhysicallyInstalledSystemMemory(out memKb); } catch { /* ignore */ }
            var installedRamInGb = memKb > 0 ? (memKb / 1024 / 1024) : 0;
            _systemDetails = $"{installedRamInGb}Gb | {(Environment.Is64BitOperatingSystem ? "64bit" : "32bit")} | {Environment.MachineName}";
        }

        public async Task EmitBatchAsync(IEnumerable<LogEvent> events)
        {
            var payload = new List<LogMessage>();

            foreach (var e in events)
            {
                payload.Add(new LogMessage
                {
                    Id = 0,
                    CreatedAt = e.Timestamp.UtcDateTime,
                    Level = e.Level switch
                    {
                        LogEventLevel.Verbose => (int)Microsoft.Extensions.Logging.LogLevel.Trace,
                        LogEventLevel.Debug => (int)Microsoft.Extensions.Logging.LogLevel.Debug,
                        LogEventLevel.Information => (int)Microsoft.Extensions.Logging.LogLevel.Information,
                        LogEventLevel.Warning => (int)Microsoft.Extensions.Logging.LogLevel.Warning,
                        LogEventLevel.Error => (int)Microsoft.Extensions.Logging.LogLevel.Error,
                        LogEventLevel.Fatal => (int)Microsoft.Extensions.Logging.LogLevel.Critical,
                        _ => (int)Microsoft.Extensions.Logging.LogLevel.Debug
                    },
                    Message = e.RenderMessage(),
                    StackTrace = e.Exception?.ToString(),
                    ProgramName = _programName,
                    ProgramVersion = _programVersion,
                    WindowsVersion = _windowsVersion,
                    SystemDetails = _systemDetails
                });
            }

            if (payload.Count == 0) return;

            var json = JsonSerializer.Serialize(payload, JsonOptions);
            using var content = new StringContent(json, Encoding.UTF8, "application/json");

            // simple retry x2
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    var resp = await _httpClient.PostAsync(_endpoint, content);
                    resp.EnsureSuccessStatusCode();
                    break;
                }
                catch (Exception ex) when (attempt < 2)
                {
                    await Task.Delay(500 * (attempt + 1));
                }
            }
        }

        public Task OnEmptyBatchAsync() => Task.CompletedTask;

        public void Dispose() => _httpClient.Dispose();
    }
}