using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Data;

namespace Shared.Services
{
    public class ApiService
    {
        // Define compiled regex patterns for better performance
        private static readonly Regex TimestampRegex = new(@"^(.*?)\s*(?=\[)", RegexOptions.Compiled);
        private static readonly Regex TagRegex = new(@"(?<=\[).+?(?=\])", RegexOptions.Compiled);
        private static readonly Regex MessageRegex = new(@"(?<=\]).*", RegexOptions.Compiled);


        private readonly HttpClient _httpClient = new HttpClient();
        private readonly string? _programName;
        private readonly string? _programVersion;
        private readonly string _windowsVersion;
        private readonly string _systemDetails;
        private const string ApiUrl = "https://ggapi.movsar.dev/Logs";
        private const int PingTimeout = 3000;

        [DllImport("kernel32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool GetPhysicallyInstalledSystemMemory(out long TotalMemoryInKilobytes);

        public ApiService()
        {

            long memKb;
            GetPhysicallyInstalledSystemMemory(out memKb);
            var installedRamInGb = memKb / 1024 / 1024;
            _programName = Assembly.GetEntryAssembly()?.GetName().Name;
            _programVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString();
            _windowsVersion = Environment.OSVersion.VersionString;
            _systemDetails = $"{installedRamInGb}Gb | {(Environment.Is64BitOperatingSystem ? "64bit" : "32bit")} | {Environment.MachineName}";
        }
        public async Task SendLogsAsync()
        {
            try
            {
                // Проверка интернета через Ping
                if (!await HasInternetConnectionAsync())
                {
                    Debug.WriteLine("Нет подключения к интернету");
                    return;
                }

                // Проверка папки с логами
                if (!Directory.Exists("logs"))
                {
                    Debug.WriteLine("Папка logs не найдена");
                    return;
                }

                // Обработка файлов
                foreach (var filePath in Directory.GetFiles("logs", "logs*.txt"))
                {
                    Debug.WriteLine($"Обработка файла: {filePath}");

                    var content = await File.ReadAllTextAsync(filePath);
                    var logs = ParseLogs(content);
                    Debug.WriteLine($"Найдено логов: {logs.Count}");

                    logs = logs.Where(l => l.Level > (int)LogLevel.Debug).ToList();

                    // 4. Отправка пачками по 50
                    foreach (var batch in logs.Chunk(50))
                    {
                        if (await SendBatch(batch.ToList()))
                        {
                            Debug.WriteLine($"логи успешно отправлены");
                        }
                        else
                        {
                            Debug.WriteLine("Ошибка при отправке логов");
                            return;
                        }
                    }

                    // Удаление файла после успешной отправки
                    var fi = new FileInfo(filePath);
                    var newPath = fi.Directory.FullName + "sent-" + fi.Name;
                    File.Move(filePath, newPath);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Ошибка: {ex.Message}");
            }
        }
        private async Task<bool> HasInternetConnectionAsync()
        {
            try
            {
                using var ping = new Ping();
                var reply = await ping.SendPingAsync("8.8.8.8", PingTimeout);
                return reply.Status == IPStatus.Success;
            }
            catch
            {
                return false;
            }
        }

        private List<LogMessage> ParseLogs(string content)
        {
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
                    ProgramName = _programName,
                    ProgramVersion = _programVersion,
                    WindowsVersion = _windowsVersion,
                    SystemDetails = _systemDetails
                });

            }

            return logs;
        }

        private async Task<bool> SendBatch(List<LogMessage> batch)
        {
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                };

                var json = JsonSerializer.Serialize(batch, options);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(ApiUrl, content);
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                return false;
            }
        }
    }
}