using EntityCache.Mapping;
using Microsoft.EntityFrameworkCore;
using System;
using EntityCache.Pooling;

namespace EntityCache.Interfaces
{
    public interface IEntityCollectionMapping
    {
        ITypeMapping TypeMapping { get; set; }

        DateTime LastPullTime { get; set; }

        //IEntity CreateEntityFromCachedEntity(DbContext context,
        //                                         CachedEntity cachedEntity,
        //                                         DateTime newSavepoint,
        //                                         ObjectPool newEntitiesPool);


        IEntity AddCachedEntityToContext(DbContext context,
                                              ICachedEntity entity,
                                              Mapper mapper,
                                              DateTime newSavepoint,
                                              ObjectPool newEntitiesPool);

        /// <summary>
        ///     Using the given context, this returns the entity with the given id from the DbSet selected for this mapping.
        /// </summary>
        IEntity FindEntity(DbContext context, string id);

        /// <summary>
        ///     This adds the given entity to the database using the given context (using the DbSet specified for this mapping).
        /// </summary>
        void AddEntity(DbContext context, IEntity entity);

        /// <summary>
        ///     This returns the entity list collection specified for this mapping.
        /// </summary>
        IEntityList GetRepositoryCollection(IDataCache repository);

        /// <summary>
        ///     This executes an action on all entities inside the DbSet that was specified for this mapping.
        /// </summary>
        /// <param name="context">the context to be used. this is used to access the DbSet and entities.</param>
        /// <param name="includeDeletedEntities">whether deleted entities should be include in the iteration</param>
        /// <param name="time">the action will be executed only on entities whose last savepoint is newer than this specified time value.</param>
        /// <param name="action">the action to be executed on the entities</param>
        void ForEachEntity(DbContext context, bool includeDeletedEntities, DateTime time, Action<IEntity> action);

        void DeleteCachedEntity(IDataCache repository, ICachedEntity cachedEntity);

        //IEntity CopyCachedEntityToEntity(DbContext context,
        //                                      CachedEntity cachedEntity,
        //                                      IEntity entity,
        //                                      DateTime newSavepoint,
        //                                      bool force);

    }
}
