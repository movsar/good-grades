using Data.Entities;
using Data.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Serilog;
using System.Runtime.CompilerServices;

namespace Data
{
    public class Storage : IDisposable
    {
        public int DB_VERSION = 1;
        public DataContext DbContext { get; private set; }
        private string _databasePath;

        public void InitializeDbContext(string databasePath, bool isNewDatabase)
        {
            try
            {
                Log.Debug("Initializing DbContext with database");
                DbContext = new DataContext() { DbPath = databasePath };
                DbContext.Database.Migrate();
                Log.Information("Database migrations applied successfully.");
            }
            catch (Exception ex)
            {
                if (ex.Message.Contains("already exists"))
                {
                    Log.Warning("Migration already exists, attempting to fix...");
                    string query = @"INSERT INTO __EFMigrationsHistory 
                                            (MigrationId, ProductVersion)
                                            VALUES ('{0}', '{1}')";

                    var query1 = string.Format(query, "20250118175903_Initial", "6.0.0");

                    DbContext.Database.ExecuteSqlRawAsync(query1);
                    DbContext.Database.Migrate();
                    Log.Information("Migration issue resolved.");
                }
                else
                {
                    Log.Fatal(ex.Message, ex.Source, ex.StackTrace, ex.InnerException);
                    throw;
                }
            }
            _databasePath = databasePath;

            // Migrate older versions to extract binary data stored in the database and write to disk
            if (isNewDatabase == false)
            {
                var currentDbVersion = DbContext.DbMetas.FirstOrDefault()?.DbVersion ?? 0;
                if (currentDbVersion < 1)
                {
                    Log.Information("Migrating from older version");
                    MigrateFromOlderVersion(databasePath);
                }
            }
        }

        private string GetRandomFileName()
        {
            return Path.GetRandomFileName() + ".gga";
        }
        private void MigrateFromOlderVersion(string databasePath)
        {
            Log.Debug("Starting migration from older version for database");
            File.Delete(databasePath + ".backup");
            File.Copy(databasePath, databasePath + ".backup");
            var dbFileName = Path.GetFileName(databasePath);
            FileInfo dbFi = new FileInfo(databasePath);
            var dbFileDir = Path.GetDirectoryName(databasePath);
            var dbAssetsDir = Directory.CreateDirectory(Path.Combine(dbFileDir, dbFileName + "-assets"));
            Log.Information("Created backup and assets directory for migration.");

            foreach (var item in DbContext.Materials)
            {
                if (item.PdfData != null)
                {
                    string fileName = GetRandomFileName();
                    item.PdfPath = fileName;

                    File.WriteAllBytes(Path.Combine(dbAssetsDir.FullName, fileName), item.PdfData);
                    item.PdfData = new byte[] { 0 };
                    Log.Debug("Migrated PDF data for material ID");
                }

                if (item.Audio != null)
                {
                    string fileName = GetRandomFileName();
                    item.AudioPath = fileName;

                    File.WriteAllBytes(Path.Combine(dbAssetsDir.FullName, fileName), item.Audio);
                    item.Audio = null;
                    Log.Debug("Migrated audio data for material ID");
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
                    Log.Debug("Migrated image data for assignment item ID");
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
                Log.Debug("Migrated background image and updated database version.");
            }
            DbContext.Database.ExecuteSqlRaw("VACUUM;");
            DbContext.SaveChanges();
            Log.Information("Migration from older version completed successfully.");
        }

        public void CreateDatabase(string databasePath)
        {

            if (File.Exists(databasePath))
            {
                Log.Warning("Database already exists, dropping existing database...");
                DropDatabase(databasePath);
            }

            InitializeDbContext(databasePath, true);
            var dbFileName = Path.GetFileName(databasePath);
            var dbFileDir = Path.GetDirectoryName(databasePath);
            var dbAssetsDir = Directory.CreateDirectory(Path.Combine(dbFileDir, dbFileName + "-assets"));

            SetDbMeta(databasePath);

            DbContext.SaveChanges();
        }
        public void SetDbMeta(string databasePath)
        {
            Log.Debug("Setting database metadata");
            var dbMeta = new DbMeta()
            {
                Title = Path.GetFileNameWithoutExtension(databasePath),
                DbVersion = DB_VERSION
            };

            DbContext.DbMetas.Add(dbMeta);
            DbContext.SaveChanges();
            Log.Information("Database metadata set successfully.");
        }
        public void DropDatabase(string dbPath)
        {
            try
            {
                Log.Debug("Attempting to drop database");
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
                Log.Error(ex, "Error dropping database");
            }
        }
        public void ImportDatabase(string filePath)
        {
            Log.Debug("Importing database");
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
                Log.Error(ex, "Error importing database");
                throw;
            }
        }

        public void Dispose()
        {
            Log.Debug("Disposing DbContext...");
            DbContext?.Dispose();
        }

        public static byte[]? ReadDbAsset(string? fileName)
        {
            var settingsService = new SettingsService();
            var dbAbsolutePath = settingsService.GetValue("lastOpenedDatabasePath");

            if (fileName == null)
            {
                Log.Warning("File name is null, cannot read asset.");
                return null;
            }

            var dbDirectory = Path.GetDirectoryName(dbAbsolutePath);
            var assetsDirectory = Path.GetFileName(dbAbsolutePath) + "-assets";

            Log.Debug("Reading asset file");
            return File.ReadAllBytes(Path.Combine(dbDirectory, assetsDirectory, fileName));
        }
    }
}