using Data;
using Serilog.Events;
using Serilog.Sinks.PeriodicBatching;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
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

        // durability
        private readonly string _bufferDir;
        private readonly long _maxBufferBytes;
        private readonly SemaphoreSlim _flushLock = new(1, 1);

        private static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            WriteIndented = false
        };

        [DllImport("kernel32.dll")]
        private static extern bool GetPhysicallyInstalledSystemMemory(out long totalMemoryKb);

        public WebApiBatchedSink(string endpoint, string bufferDirectory, long maxBufferBytes)
        {
            _endpoint = new Uri(endpoint);
            _httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(10)
            };

            _bufferDir = bufferDirectory;
            Directory.CreateDirectory(_bufferDir);

            // Safety floor 5MB
            _maxBufferBytes = Math.Max(maxBufferBytes, 5 * 1024 * 1024);

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
            // Always try to flush buffered files first
            await TryFlushBufferAsync();

            // Send current batch
            var payload = Map(events);
            if (payload.Count == 0) return;

            var ok = await TryPostAsync(payload);
            if (!ok)
            {
                await PersistAsync(payload);
            }
        }

        public async Task OnEmptyBatchAsync()
        {
            // Even if nothing new, keep trying to flush old buffered logs
            await TryFlushBufferAsync();
        }

        private List<LogMessage> Map(IEnumerable<LogEvent> events)
        {
            var list = new List<LogMessage>();
            foreach (var e in events)
            {
                list.Add(new LogMessage
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
            return list;
        }

        private async Task<bool> TryPostAsync(List<LogMessage> payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload, JsonOptions);
                using var content = new StringContent(json, Encoding.UTF8, "application/json");

                var resp = await _httpClient.PostAsync(_endpoint, content);
                resp.EnsureSuccessStatusCode();
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                await Task.Delay(500);
                return false;
            }
        }

        private async Task PersistAsync(List<LogMessage> payload)
        {
            try
            {
                // enforce max buffer size (best-effort)
                TrimBufferIfTooLarge();

                var fileName = $"{DateTime.UtcNow:yyyyMMddHHmmssfff}-{Guid.NewGuid():N}.json";
                var path = Path.Combine(_bufferDir, fileName);
                var json = JsonSerializer.Serialize(payload, JsonOptions);
                await File.WriteAllTextAsync(path, json, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                // Swallow: never crash the app due to logging persistence
                Debug.WriteLine(ex);
            }
        }

        private void TrimBufferIfTooLarge()
        {
            try
            {
                var files = new DirectoryInfo(_bufferDir)
                    .GetFiles("*.json")
                    .OrderBy(f => f.CreationTimeUtc)
                    .ToList();

                long total = files.Sum(f => f.Length);
                if (total <= _maxBufferBytes)
                    return;

                foreach (var f in files)
                {
                    try { f.Delete(); } catch { /* ignore */ }

                    total -= f.Length;

                    if (total <= _maxBufferBytes)
                        break;
                }
            }
            catch { /* ignore */ }
        }

        private async Task TryFlushBufferAsync()
        {
            // Serialize flush attempts so we don't race with concurrent batch ticks
            if (!await _flushLock.WaitAsync(0))
                return;

            try
            {
                var di = new DirectoryInfo(_bufferDir);
                if (!di.Exists)
                    return;

                // oldest-first for stable ordering
                var files = di.GetFiles("*.json")
                    .OrderBy(f => f.CreationTimeUtc)
                    .ToList();

                foreach (var f in files)
                {
                    List<LogMessage>? payload = null;
                    try
                    {
                        var json = await File.ReadAllTextAsync(f.FullName, Encoding.UTF8);
                        payload = JsonSerializer.Deserialize<List<LogMessage>>(json, JsonOptions);
                    }
                    catch
                    {
                        // Corrupted? delete and continue
                        try { f.Delete(); } catch { }
                        continue;
                    }

                    if (payload is null || payload.Count == 0)
                    {
                        try { f.Delete(); } catch { }
                        continue;
                    }

                    var ok = await TryPostAsync(payload);
                    if (ok)
                    {
                        try { f.Delete(); } catch { }
                    }
                    else
                    {
                        // Still offline / server down; stop and keep the rest
                        break;
                    }
                }
            }
            finally
            {
                _flushLock.Release();
            }
        }

        public void Dispose()
        {
            _httpClient.Dispose();
            _flushLock.Dispose();
        }
    }
}
