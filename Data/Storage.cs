﻿using Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Data
{
    public class Storage
    {
        public DataContext DbContext => new DataContext();

        private ILogger _logger;
        public Storage(ILogger<Storage> logger)
        {
            _logger = logger;
        }

        public void SetDatabaseConfig(string databasePath)
        {
            try
            {
                using (var context = new DataContext(databasePath))
                {
                    context.Database.EnsureCreated();
                }
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex.Message, ex.Source, ex.StackTrace, ex.InnerException);
                throw;
                //return false;
            }
        }
        public void CreateDatabase(string databasePath, string? appVersion)
        {
            if (File.Exists(databasePath))
            {
                DropDatabase(databasePath);
            }

            SetDatabaseConfig(databasePath);

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
                DbContext.Database.EnsureDeleted();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message, ex.StackTrace, ex.InnerException);
            }
        }
        public void ImportDatabase(string filePath)
        {
            var dbToImport = new DataContext(filePath);
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
