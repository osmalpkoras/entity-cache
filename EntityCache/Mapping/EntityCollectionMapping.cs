using System;
using System.Linq;
using System.Reflection;
using EntityCache.Interfaces;
using EntityCache.Pooling;
using Microsoft.EntityFrameworkCore;

namespace EntityCache.Mapping
{
    /// <summary>
    ///     This class maps a database collection of type <see cref="DbSet{TEntity}" /> to an entity list collection of type
    ///     <see cref="EntityList{TEntity}" />.
    ///     The database collection must be of a type derived from <see cref="IEntity" />, while the entity list collection
    ///     must be of a type derived from <see cref="ICachedEntity" />.
    ///     This class also encapsulates basic operations like finding, adding, deleting entities from/to the database or
    ///     entity list collection.
    /// </summary>
    public class EntityCollectionMapping<TCachedEntity, TEntity> : IEntityCollectionMapping
        where TCachedEntity : ICachedEntity where TEntity : class, IEntity
    {
        internal PropertyInfo ContextProperty;
        internal PropertyInfo RepositoryProperty;

        public EntityCollectionMapping()
        {
            IncludeAllEntityReferences = entities =>
                                         {
                                             foreach (var propertyMapping in TypeMapping
                                                 .ReferencedEntityPropertyMappings)
                                             {
                                                 entities = entities.Include(propertyMapping.EntityProperty.Name);
                                             }

                                             return entities;
                                         };
        }

        public Func<IQueryable<TEntity>, IQueryable<TEntity>> IncludeAllEntityReferences { get; set; }
        public ITypeMapping TypeMapping { get; set; }

        public DateTime LastPullTime { get; set; } = DateTime.MinValue;

        public IEntity AddCachedEntityToContext(DbContext context,
                                                ICachedEntity entity,
                                                Mapper mapper,
                                                DateTime newSavepoint,
                                                ObjectPool newEntitiesPool)
        {
            var pooledDbEntity = newEntitiesPool.Get(TypeMapping.EntityType, entity.Id);
            if (pooledDbEntity == null)
            {
                pooledDbEntity
                    = mapper.CreateEntityFromCachedEntity(context, entity, TypeMapping, newSavepoint, newEntitiesPool);
                AddEntity(context, pooledDbEntity);
            }

            return pooledDbEntity;
        }

        /// <summary>
        ///     This returns the entity list collection specified for this mapping.
        /// </summary>
        public IEntityList GetRepositoryCollection(IDataCache repository) =>
            repository.Mapper.CachedEntityPool.GetPool<TCachedEntity>();

        /// <summary>
        ///     This executes an action on all entities inside the DbSet that was specified for this mapping.
        /// </summary>
        /// <param name="context">the context to be used. this is used to access the DbSet and entities.</param>
        /// <param name="includeDeletedEntities">whether deleted entities should be include in the iteration</param>
        /// <param name="time">
        ///     the action will be executed only on entities whose last savepoint is newer than this specified time
        ///     value.
        /// </param>
        /// <param name="action">the action to be executed on the entities</param>
        public void ForEachEntity(DbContext context, bool includeDeletedEntities, DateTime time, Action<IEntity> action)
        {
            var dbProperty = GetContextCollection(context);
            IQueryable<TEntity> dbCollection = dbProperty;
            if (!includeDeletedEntities)
            {
                dbCollection = dbProperty.Where(e => !e.IsDeleted);
            }

            dbCollection = IncludeAllEntityReferences(dbCollection);
            dbCollection
                .Where(e => e.UtcTimeStamp > time)
                .ToList()
                .ForEach(action);
        }

        public void DeleteCachedEntity(IDataCache repository, ICachedEntity cachedEntity)
        {
            var repoCollection = (EntityList<TCachedEntity>) GetRepositoryCollection(repository);
            repoCollection.Remove((TCachedEntity) cachedEntity);
            repoCollection.RemovedEntities.Remove((TCachedEntity) cachedEntity);
        }


        /// <summary>
        ///     Using the given context, this returns the entity with the given id from the DbSet selected for this mapping.
        /// </summary>
        public IEntity FindEntity(DbContext context, string id)
        {
            var dbSet = GetContextCollection(context);
            return IncludeAllEntityReferences(dbSet).FirstOrDefault(e => e.Id == id);
        }

        /// <summary>
        ///     This adds the given entity to the database using the given context (using the DbSet specified for this mapping).
        /// </summary>
        public void AddEntity(DbContext context, IEntity entity)
        {
            var dbSet = GetContextCollection(context);
            dbSet.Add((TEntity) entity);
        }

        public DbSet<TEntity> GetContextCollection(DbContext context) =>
            (DbSet<TEntity>) ContextProperty.GetValue(context);
    }
}