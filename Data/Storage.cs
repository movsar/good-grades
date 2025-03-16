using Data.Entities;
using Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Runtime.CompilerServices;

namespace Data
{
    public class Storage : IDisposable
    {
        public int DB_VERSION = 1;
        public DataContext DbContext { get; private set; }

        private ILogger _logger;
        private string _databasePath;

        public Storage(ILogger<Storage> logger)
        {
            _logger = logger;
        }

        public void SetDatabaseConfig(string databasePath)
        {
            try
            {
                DbContext = new DataContext() { DbPath = databasePath };
                DbContext.Database.Migrate();
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    string query = @"INSERT INTO __EFMigrationsHistory 
                                            (MigrationId, ProductVersion)
                                            VALUES ('{0}', '{1}')";

                    var query1 = string.Format(query, "20250118175903_Initial", "6.0.0");

                    DbContext.Database.ExecuteSqlRawAsync(query1);
                    DbContext.Database.Migrate();
                }
                else
                {
                    _logger.LogCritical(ex.Message, ex.Source, ex.StackTrace, ex.InnerException);
                    throw;
                }
            }

            _databasePath = databasePath;

            var currentDbVersion = DbContext.DbMetas.FirstOrDefault()?.DbVersion ?? 0;
            if (currentDbVersion < 1)
            {
                _logger.LogInformation("Migrating from older version");
                MigrateFromOlderVersion(databasePath);
            }
        }
        private string GetRandomFileName()
        {
            return Path.GetRandomFileName() + ".gga";
        }
        private void MigrateFromOlderVersion(string databasePath)
        {
            File.Copy(databasePath, databasePath + ".backup");
            var dbFileName = Path.GetFileName(databasePath);
            FileInfo dbFi = new FileInfo(databasePath);
            var dbFileDir = Path.GetDirectoryName(databasePath);
            var dbAssetsDir = Directory.CreateDirectory(Path.Combine(dbFileDir, dbFileName + "-assets"));

            foreach (var item in DbContext.Materials)
            {
                if (item.PdfData != null)
                {
                    string fileName = GetRandomFileName();
                    item.PdfPath = fileName;

                    File.WriteAllBytes(Path.Combine(dbAssetsDir.FullName, fileName), item.PdfData);
                    item.PdfData = new byte[] { 0 };
                }

                if (item.Audio != null)
                {
                    string fileName = GetRandomFileName();
                    item.AudioPath = fileName;

                    File.WriteAllBytes(Path.Combine(dbAssetsDir.FullName, fileName), item.Audio);
                    item.Audio = null;
                }
            }

            foreach (var item in DbContext.AssignmentItems)
            {
                if (item.Image != null)
                {
                    string fileName = GetRandomFileName();
                    item.ImagePath = fileName;

                    File.WriteAllBytes(Path.Combine(dbAssetsDir.FullName, fileName), item.Image);
                    item.Image = null;
                }
            }

            var dbMeta = DbContext.DbMetas.FirstOrDefault();
            if (dbMeta != null && dbMeta.BackgroundImage != null)
            {
                string fileName = GetRandomFileName();
                dbMeta.BackgroundImagePath = fileName;

                File.WriteAllBytes(Path.Combine(dbAssetsDir.FullName, fileName), dbMeta.BackgroundImage);
                dbMeta.BackgroundImage = null;

                dbMeta.DbVersion = DB_VERSION;
            }
            DbContext.Database.ExecuteSqlRaw("VACUUM;");
            DbContext.SaveChanges();
        }

        public void CreateDatabase(string databasePath)
        {
            if (File.Exists(databasePath))
            {
                DropDatabase(databasePath);
            }

            SetDatabaseConfig(databasePath);

            SetDbMeta(databasePath);

            DbContext.SaveChanges();
        }
        public void SetDbMeta(string databasePath)
        {
            var dbMeta = new DbMeta()
            {
                Title = Path.GetFileNameWithoutExtension(databasePath),
                DbVersion = DB_VERSION
            };

            DbContext.DbMetas.Add(dbMeta);
            DbContext.SaveChanges();
        }
        public void DropDatabase(string dbPath)
        {
            try
            {
                if (DbContext == null || DbContext.Database == null)
                {
                    File.Delete(dbPath);
                }
                else
                {
                    DbContext.Database.EnsureDeleted();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace, ex.InnerException);
            }
        }
        public void ImportDatabase(string filePath)
        {
            var dbToImport = new DataContext() { DbPath = filePath };
            var segments = dbToImport.Segments;

            try
            {
                foreach (Segment segment in segments)
                {
                    DbContext.Segments.Add(segment);
                }
                DbContext.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace, ex.InnerException);
                throw;
            }
        }

        public void Dispose()
        {
            DbContext?.Dispose();
        }

        public static byte[]? ReadDbAsset(string? fileName)
        {
            var settingsService = new SettingsService();
            var dbAbsolutePath = settingsService.GetValue("lastOpenedDatabasePath");

            if (fileName == null)
            {
                return null;
            }

            var dbDirectory = Path.GetDirectoryName(dbAbsolutePath);
            var assetsDirectory = Path.GetFileName(dbAbsolutePath) + "-assets";

            return File.ReadAllBytes(Path.Combine(dbDirectory, assetsDirectory, fileName));
        }
    }
}
