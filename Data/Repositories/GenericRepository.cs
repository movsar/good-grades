﻿using Data.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Data.Repositories
{
    public class GenericRepository<TEntity> : IGeneralRepository where TEntity : class, IEntityBase, new()
    {
        protected DataContext _dbContext;
        internal GenericRepository()
        {
            _dbContext = Storage.GetDataContext();
        }

        protected IEnumerable<TEntity> GetEntitiesByIds(string[] ids)
        {
            foreach (var entity in _dbContext.Set<TEntity>())
            {
                if (ids.Any(id => id == entity.Id))
                {
                    yield return entity;
                }
            }
        }

        #region Generic CRUD

        public virtual void Add<TModel>(ref TModel model) where TModel : IModelBase
        {
            dynamic entity = new TEntity();
            entity.SetFromModel(model);

            _dbContext.Add(entity);
            _dbContext.SaveChanges();

            model = entity.ToModel();
        }

        public virtual void Update<TModel>(TModel model) where TModel : IModelBase
        {
            var result = _dbContext.Set<TEntity>().Find(typeof(TEntity), model.Id);            
            result.SetFromModel(model);
            
            _dbContext.SaveChanges();
        }

        public virtual void Delete<TModel>(TModel model) where TModel : IModelBase
        {
            var result = _dbContext.Set<TEntity>().Find(typeof(TEntity), model.Id);
            _dbContext.Remove(result);

            _dbContext.SaveChanges();
        }

        public TModel GetById<TModel>(string id)
        {
            var result = _dbContext.Set<TEntity>().Find(typeof(TEntity), id);
            return (TModel)result.ToModel();
        }

        public virtual IEnumerable<TModel> GetByIds<TModel>(string[] ids) where TModel : IModelBase
        {
            var entries = GetEntitiesByIds(ids);
            return EntitiesToModels<TEntity, TModel>(entries);
        }

        public virtual IEnumerable<TModel> GetAll<TModel>() where TModel : IModelBase
        {
            var query = _dbContext.Set<TEntity>().AsQueryable();

            // Include all navigation properties
            var navigations = _dbContext.Model.FindEntityType(typeof(TEntity))
                                 .GetNavigations()
                                 .Select(e => e.Name);

            foreach (var property in navigations)
            {
                query = query.Include(property);
            }

            var result = query.ToList(); // Execute the query

            return EntitiesToModels<TEntity, TModel>(result);
        }
        #endregion

        public IEnumerable<TTarget> EntitiesToModels<TSource, TTarget>(IEnumerable<TSource> realmObjects)
        {
            var collection = (IEnumerable<IEntityBase>)realmObjects;

            foreach (var item in collection)
            {
                yield return (TTarget)item.ToModel();
            }
        }
    }
}
