using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.NetworkInformation;
using System.Reflection;
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
        private readonly HttpClient _httpClient = new HttpClient();
        private const string ApiUrl = "https://ggapi.movsar.dev/Logs";
        private const int PingTimeout = 3000;

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
                foreach (var file in Directory.GetFiles("logs", "logs*.txt"))
                {
                    Debug.WriteLine($"Обработка файла: {file}");

                    var content = await File.ReadAllTextAsync(file);
                    var logs = ParseLogs(content);
                    Debug.WriteLine($"Найдено логов: {logs.Count}");

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
                    File.Delete(file);
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
                var stackTrace = lines.Length > 1 ? string.Join(Environment.NewLine, lines.Skip(1)) : null;

                logs.Add(new LogMessage
                {
                    Id = 0,
                    Message = lines[0].Length > 19 ? lines[0].Substring(19).Trim() : lines[0],
                    StackTrace = stackTrace,
                    CreatedAt = DateTime.TryParse(lines[0].Substring(0, Math.Min(19, lines[0].Length)), out var date)
                              ? date
                              : DateTime.Now,
                    Level = lines[0].Contains("[ERR]") ? (int)LogLevel.Error :
                           lines[0].Contains("[WRN]") ? (int)LogLevel.Warning :
                           lines[0].Contains("[INF]") ? (int)LogLevel.Information :
                           (int)LogLevel.Debug,
                    ProgramName = Assembly.GetEntryAssembly()?.GetName().Name,
                    ProgramVersion = Assembly.GetEntryAssembly()?.GetName().Version?.ToString(),
                    WindowsVersion = Environment.OSVersion.VersionString,
                    SystemDetails = $"{Environment.OSVersion} | {Environment.MachineName} | " +
                                  $"{Environment.ProcessorCount} cores | " +
                                  $"{(Environment.Is64BitOperatingSystem ? "64-bit" : "32-bit")}"
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
            catch
            {
                return false;
            }
        }
    }
}