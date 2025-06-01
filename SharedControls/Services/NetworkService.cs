using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Velopack.Sources;

namespace Shared.Services
{
    public static class NetworkService
    {
        private static HttpClient client = new HttpClient();
        private const string url = "https://api.github.com/repos/movsar/good-grades/releases";
        static NetworkService()
        {
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.UserAgent.ParseAdd("Good Grades/1.0");
        }

        public static async Task<GithubRelease> GetLatestRelease(string module)
        {
            if (module != "manager" && module != "player")
            {
                throw new Exception("Wrong module name");
            }

            try
            {
                var response = await client.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var responseBody = await response.Content.ReadAsStringAsync();

                var releases = JsonSerializer.Deserialize<List<GithubRelease>>(responseBody, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (releases == null)
                {
                    throw new();
                }

                var latestVersion = releases.Where(r => r.Name!.ToLower().Contains(module))
                                            .Where(r => r.Assets.Any(a => a.Name != null && a.Name.ToLower().Contains("setup")))
                                            .OrderByDescending(r => r.PublishedAt)
                                            .FirstOrDefault();

                if (latestVersion == null)
                {
                    throw new();
                }

                return latestVersion;
            }
            catch (Exception ex)
            {
                throw new Exception("Couldn't fetch latest version information", ex);
            }
        }

        internal static async Task DownloadUpdate(string remotePath, string localPath)
        {
            var response = await client.GetAsync(remotePath, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();

            using var contentStream = await response.Content.ReadAsStreamAsync();
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var progress = new Progress<double>();
            var cancellationTokenSource = new CancellationTokenSource();

            var downloadWindow = new DownloadProgressWindow(cancellationTokenSource);
            downloadWindow.Show();

            progress.ProgressChanged += (s, percent) =>
            {
                downloadWindow.UpdateProgress(percent);
            };

            try
            {
                await DownloadWithProgress(contentStream, localPath, totalBytes, progress, cancellationTokenSource.Token);
            }
            catch (OperationCanceledException)
            {
                return ;
            }
            finally
            {
                downloadWindow.Close();
            }
        }

        private static async Task DownloadWithProgress(Stream contentStream, string filePath, long totalBytes, IProgress<double> progress, CancellationToken cancellationToken)
        {
            const int bufferSize = 8192;
            var buffer = new byte[bufferSize];
            var totalRead = 0L;
            int bytesRead;

            using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None, bufferSize, true);

            while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, cancellationToken)) != 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead), cancellationToken);
                totalRead += bytesRead;

                if (totalBytes > 0)
                {
                    // Report actual download progress here
                    progress.Report((double)totalRead / totalBytes * 100);
                }
            }
        }


    }
}
