using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Data
{
    public class Storage
    {
        public int DB_VERSION = 1;
        public DataContext DbContext;

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

        private void MigrateFromOlderVersion(string databasePath)
        {
            var dbFileName = Path.GetFileName(databasePath);
            var dbFileDir = Path.GetDirectoryName(databasePath);
            var dbAssetsDir = Directory.CreateDirectory(Path.Combine(dbFileDir, dbFileName + "-assets"));

            foreach (var material in DbContext.Materials)
            {
                if (material.PdfData != null)
                {
                    string path = Path.Combine(dbAssetsDir.FullName, Path.GetRandomFileName());

                    File.WriteAllBytes(path, material.PdfData);

                    material.PdfPath = path;
                    material.PdfData = new byte[] { 0 };
                }

                if (material.Audio != null)
                {
                    string path = Path.Combine(dbAssetsDir.FullName, Path.GetRandomFileName());

                    File.WriteAllBytes(path, material.Audio);

                    material.AudioPath = path;
                    material.Audio = null;
                }
            }

            foreach (var item in DbContext.AssignmentItems)
            {
                if (item.Image != null)
                {
                    string path = Path.Combine(dbAssetsDir.FullName, Path.GetRandomFileName());
                    File.WriteAllBytes(path, item.Image);

                    item.ImagePath = path;
                    item.Image = null;
                }
            }

            var dbMeta = DbContext.DbMetas.FirstOrDefault();
            if (dbMeta != null && dbMeta.BackgroundImage != null)
            {
                string path = Path.Combine(dbAssetsDir.FullName, Path.GetRandomFileName());

                File.WriteAllBytes(path, dbMeta.BackgroundImage);

                dbMeta.BackgroundImagePath = path;
                dbMeta.BackgroundImage = null;
                dbMeta.DbVersion = DB_VERSION;
            }

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
    }
}
