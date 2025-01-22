using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Reflection;

namespace Data
{
    public class Storage
    {
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
            string? appVersion = Assembly.GetExecutingAssembly().GetName().Version?.ToString();

            var dbMeta = new DbMeta()
            {
                Title = Path.GetFileNameWithoutExtension(databasePath),
                AppVersion = appVersion
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
