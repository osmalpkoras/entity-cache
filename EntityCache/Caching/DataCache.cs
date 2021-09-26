using EntityCache.Exceptions;
using EntityCache.Interfaces;
using EntityCache.Mapping;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EntityCache.Pooling;

namespace EntityCache.Caching
{
    public abstract class DataCache : IDataCache
    {

        /// <summary>
        ///     This methods adds the given entity and all referenced entities recursively.
        ///     Use this method if you can't add all new entities separately (or for comfort).
        ///     The type mapping must be contained in <see cref="TypeMappingBase._typeMappings"/>!
        /// </summary>
        public void AddCachedEntity(ICachedEntity cachedEntity)
        {
            var visitedEntities = new List<ICachedEntity>();
            AddCachedEntityRecursively(cachedEntity, visitedEntities);
        }

        private void AddCachedEntityRecursively(ICachedEntity cachedEntity, ICollection<ICachedEntity> visitedObjects)
        {
            if (cachedEntity == null || visitedObjects.Contains(cachedEntity))
            {
                return;
            }
            visitedObjects.Add(cachedEntity);

            if (Mapper.CachedEntityPool.Get(cachedEntity.GetType(), cachedEntity.Id) == null)
            {
                Mapper.CachedEntityPool.Add(cachedEntity.GetType(), cachedEntity);
            }

            var mapping = Mapper.Configuration.GetTypeMappingByCachedEntityType(cachedEntity.GetType());
            foreach (IPropertyMapping propertyMapping in mapping.ReferencedEntityPropertyMappings)
            {
                AddCachedEntityRecursively((ICachedEntity)propertyMapping.GetValue(cachedEntity), visitedObjects);
            }
        }
        
        // TODO: es fehlen ncoh typenparameter, damit nicht die falsche configuration für die den falschen IDataSource bzw. den falschen IDataCache übergeben werden kann
        public DataCache(IMappingConfiguration configuration, IDataSource database)
        {
            Database = database;
            Mapper = new Mapper(configuration);
        }

        public IDataSource Database { get; }
        public Mapper Mapper { get; }

        public List<PullConflicts> Pull(bool force = false)
        {
            var conflicts = new List<PullConflicts>();
            if (Database.Exists)
            {
                using (DbContext context = Database.CreateDbContext())
                {
                    var referencedCachedEntities = new List<ICachedEntity>();
                    foreach (IEntityCollectionMapping entityCollectionMappingBase in Mapper.Configuration.EntityMappings)
                    {
                        referencedCachedEntities.AddRange(PullFromContext(context, entityCollectionMappingBase, conflicts, force));
                    }

                    foreach (ICachedEntity referencedCachedEntity in referencedCachedEntities)
                    {
                        IEntityCollectionMapping mapping =
                            Mapper.Configuration.GetEntityMappingByCachedEntityType(referencedCachedEntity.GetType());
                        Mapper.PullCachedEntityAndReferencesFromContext(this, context, referencedCachedEntity, mapping, conflicts,
                                                               force);
                    }
                }
            }

            return conflicts;
        }

        public List<PullConflicts> Pull(ICachedEntity cachedEntity, bool force = false)
        {
            var conflicts = new List<PullConflicts>();
            if (cachedEntity.IsNew) return conflicts; // nothing to do if the object is new and doesn't exist in database

            if (Database.Exists)
            {
                using (DbContext context = Database.CreateDbContext())
                {
                    IEntityCollectionMapping mapping = Mapper.Configuration.GetEntityMappingByCachedEntityType(cachedEntity.GetType());

                    List<ICachedEntity> referencedCachedEntities =
                        PullCachedEntityFromContext(context, cachedEntity, mapping, conflicts, force);

                    foreach (ICachedEntity referencedCachedEntity in referencedCachedEntities)
                    {
                        Mapper.PullCachedEntityAndReferencesFromContext(this, context, referencedCachedEntity, mapping, conflicts,
                                                               force);
                    }

                    return conflicts;
                }
            }

            return conflicts;
        }


        public List<PullConflicts> Pull<TCachedEntity>(bool force = false) where TCachedEntity : ICachedEntity
        {
            var conflicts = new List<PullConflicts>();
            if (Database.Exists)
            {
                using (DbContext context = Database.CreateDbContext())
                {
                    var referencedCachedEntities = new List<ICachedEntity>();
                    referencedCachedEntities.AddRange(PullFromContext<TCachedEntity>(context, conflicts, force));

                    foreach (ICachedEntity referencedCachedEntity in referencedCachedEntities)
                    {
                        IEntityCollectionMapping mapping =
                            Mapper.Configuration.GetEntityMappingByCachedEntityType(referencedCachedEntity.GetType());
                        Mapper.PullCachedEntityAndReferencesFromContext(this, context, referencedCachedEntity, mapping, conflicts,
                                                               force);
                    }

                    return conflicts;
                }
            }

            return conflicts;
        }

        public bool Push(bool force = false)
        {
            if (!Database.Exists && !Database.DeployDatabase())
            {
                return false;
            }

            using (DbContext context = Database.CreateDbContext())
            {
                var newSavepoint = DateTime.UtcNow;
                var newEntitiesPool = new ObjectPool();

                // we first push all new entities to the database and save the changes
                // this will ensure that entities, that are referenced by an entity with updates, can be retrieved from the database context
                // as it is done below in PushUpdatesToContext!
                foreach (IEntityCollectionMapping entityCollectionMappingBase in Mapper.Configuration.EntityMappings)
                {
                    PushNewEntitiesToContext(context, entityCollectionMappingBase, newSavepoint, newEntitiesPool);
                }

                // make all new entities persistent
                if (!Database.SaveChanges(context))
                {
                    return false;
                }

                // if there are entities whose ID are assigned only after insertion into database,
                // then these entities need to be handled after all pushes and before a return
                // the least one must do is to write the new entity id back into the respective domain object
                foreach (IEntityCollectionMapping entityCollectionMappingBase in Mapper.Configuration.EntityMappings)
                {
                    // the push fails when at least one push fails.
                    if (!PushUpdatesToContext(context, entityCollectionMappingBase, newSavepoint, force))
                    {
                        return false;
                    }
                }

                return true;
            }
        }


        public bool Push<TCachedEntity>(bool force = false) where TCachedEntity : ICachedEntity
        {
            if (!Database.Exists && !Database.DeployDatabase())
            {
                return false;
            }
            using (DbContext context = Database.CreateDbContext())
            {
                var newSavepoint = DateTime.UtcNow;
                var newEntitiesPool = new ObjectPool();

                PushNewEntitiesToContext<TCachedEntity>(context, newSavepoint, newEntitiesPool);
                if (!Database.SaveChanges(context))
                {
                    return false;
                }

                return PushUpdatesToContext<TCachedEntity>(context, newSavepoint, force);
            }
        }

        public bool Push(ICachedEntity cachedEntity, bool force = false)
        {
            if (!Database.Exists && !Database.DeployDatabase())
            {
                return false;
            }
            using (DbContext context = Database.CreateDbContext())
            {
                var newSavepoint = DateTime.UtcNow;
                var newEntitiesPool = new ObjectPool();
                IEntityCollectionMapping mapping = Mapper.Configuration.GetEntityMappingByCachedEntityType(cachedEntity.GetType());

                if (cachedEntity.IsNew && mapping.AddCachedEntityToContext(context, cachedEntity, Mapper, newSavepoint, newEntitiesPool) == null)
                {
                    return false;
                }

                if (!Database.SaveChanges(context))
                {
                    return false;
                }

                return PushCachedEntityToContext(context, mapping, cachedEntity, newSavepoint, force) != null
                    && Database.SaveChanges(context);
            }
        }

        public bool Synchronize(Action<IEnumerable<PullConflicts>> resolveConflictsCallback)
        {
            // if the database exists, we need to pull data first in order to resolve conflicts before we push our data
            if (Database.Exists)
            {
                // first we pull all entities without overriding local properties that have no backing field
                List<PullConflicts> conflicts = Pull(false);

                // then we resolve existing conflicts
                resolveConflictsCallback(conflicts);
            }

            // now we push whatever we have, even properties that don't have backing fields
            // pushing when the database does not exist will create a new one
            return Push(true);
        }

        public bool Synchronize<TCachedEntity>(Action<IEnumerable<PullConflicts>> resolveConflictsCallback)
            where TCachedEntity : ICachedEntity
        {
            // if the database exists, we need to pull data first in order to resolve conflicts before we push our data
            if (Database.Exists)
            {
                // first we pull all entities without overriding local properties that have no backing field
                List<PullConflicts> conflicts = Pull<TCachedEntity>();

                // then we resolve existing conflicts
                resolveConflictsCallback(conflicts);
            }

            // now we push whatever we have, even properties that don't have backing fields
            // pushing when the database does not exist will create a new one
            return Push<TCachedEntity>(true);
        }

        public bool Synchronize(ICachedEntity cachedEntity,
                                Action<IEnumerable<PullConflicts>> resolveConflictsCallback)
        {
            // if the database exists, we need to pull data first in order to resolve conflicts before we push our data
            if (Database.Exists)
            {
                // first we pull all entities without overriding local properties that have no backing field
                List<PullConflicts> conflicts = Pull(cachedEntity);

                // then we resolve existing conflicts
                resolveConflictsCallback(conflicts);
            }

            // now we push whatever we have, even properties that don't have backing fields
            // pushing when the database does not exist will create a new one
            return Push(cachedEntity, true);
        }


        /// <summary>
        ///     When pulling a domain object, the entity might reference entities that have not been loaded before.
        ///     In this case a new domain object will be created and added to the Repository.CachedEntityPool.
        ///     It will also be returned, so that it can be handled be the calling code.
        ///     The returned domain objects need to be parsed to explicitly pull all these new domain objects and related objects (if they are not in the Repository.CachedEntityPool already)
        /// </summary>
        private List<ICachedEntity> PullCachedEntityFromContext(DbContext context,
                                                         ICachedEntity cachedEntity,
                                                         IEntityCollectionMapping mapping,
                                                         List<PullConflicts> conflicts,
                                                         bool force)
        {
            IEntity entity = mapping.FindEntity(context, cachedEntity.Id);
            if (Mapper.DoesCachedEntityNeedUpdate(cachedEntity, entity))
            {
                return Mapper.PullCachedEntityFromEntity(this, cachedEntity, entity, mapping, conflicts, force);
            }

            return new List<ICachedEntity>();
        }

        private bool PushUpdatesToContext<TCachedEntity>(DbContext context, DateTime newSavepoint, bool force)
            where TCachedEntity : ICachedEntity
        {
            IEntityCollectionMapping mapping = Mapper.Configuration.GetEntityMappingByCachedEntityType(typeof(TCachedEntity));
            return PushUpdatesToContext(context, mapping, newSavepoint, force);
        }

        private bool PushUpdatesToContext(DbContext context,
                                    IEntityCollectionMapping mapping,
                                    DateTime newSavepoint,
                                    bool force)
        {
            IEntityList repoCollection = mapping.GetRepositoryCollection(this);
            if (repoCollection == null)
            {
                repoCollection = Mapper.CachedEntityPool.CreatePool(mapping.TypeMapping.CachedEntityType).Collection;
            }

            // iterate over all dirty entities
            foreach (ICachedEntity cachedEntity in GetDirtyEntities(mapping.TypeMapping.CachedEntityType, repoCollection))
            {
                PushCachedEntityToContext(context, mapping, cachedEntity, newSavepoint, force);

                if (!Database.SaveChanges(context))
                {
                    return false;
                }
            }

            // handle all locally deleted entities
            foreach (ICachedEntity cachedEntity in repoCollection.GetRemovedEntities().Cast<ICachedEntity>())
            {
                // if the entity has been deleted remotely already, we wont touch it anymore.
                // the last user to touch an entity is the one who deleted it
                if (mapping.FindEntity(context, cachedEntity.Id) is IEntity entity && !entity.IsDeleted)
                {
                    Mapper.PullCachedEntityFromEntity(this, cachedEntity, entity, mapping, new List<PullConflicts>(), false);
                    // when we delete an entity, we want to take local changes for conflicts by default
                    // because when the entity is restored, it should be restored in the state
                    // that includes changes applied by the user who wanted to delete the entity
                    cachedEntity.ForEachConflictingProperty((field, info) => field.TakeLocal());
                    entity = PushCachedEntityToContext(context, mapping, cachedEntity, newSavepoint, force);
                    entity.IsDeleted = true;
                    entity.UtcTimeStamp = newSavepoint;

                    if (!Database.SaveChanges(context))
                    {
                        return false;
                    }
                }
            }

            repoCollection.ClearRemovedEntities();
            return true;
        }


        /// <summary>
        ///     This method assumes that the entity referred to by the given domain object has already been added to the database (calling SaveChanges on the context is not required)
        ///     Referenced entities are not pushed recursively.
        /// </summary>
        /// <returns>the entity corresponding to the domain object</returns>
        private IEntity PushCachedEntityToContext(DbContext context,
                                               IEntityCollectionMapping entityMapping,
                                               ICachedEntity cachedEntity,
                                               DateTime newSavepoint,
                                               bool force)
        {
            IEntity entity = entityMapping.FindEntity(context, cachedEntity.Id);
            return Mapper.CopyCachedEntityToEntity(context, cachedEntity, entity, entityMapping.TypeMapping, newSavepoint, force);
        }

        private void PushNewEntitiesToContext<TCachedEntity>(DbContext context,
                                                           DateTime newSavepoint,
                                                           ObjectPool newEntitiesPool) where TCachedEntity : ICachedEntity =>
            PushNewEntitiesToContext(context, Mapper.Configuration.GetEntityMappingByCachedEntityType(typeof(TCachedEntity)), newSavepoint, newEntitiesPool);

        private void PushNewEntitiesToContext(DbContext context,
                                              IEntityCollectionMapping mapping,
                                              DateTime newSavepoint,
                                              ObjectPool newEntitiesPool)
        {
            IEntityList repoCollection = mapping.GetRepositoryCollection(this);
            if (repoCollection == null)
            {
                repoCollection = Mapper.CachedEntityPool.CreatePool(mapping.TypeMapping.CachedEntityType).Collection;
            }

            // handle all new facts.
            foreach (ICachedEntity newCachedEntity in repoCollection)
            {
                if (newCachedEntity.IsNew)
                {
                    mapping.AddCachedEntityToContext(context, newCachedEntity, Mapper, newSavepoint, newEntitiesPool);
                }
            }
        }

        /// <summary>
        ///     this method will get all dirty entities from a collection using reflection.
        ///     being dirty refers to having at least one change in a cached property
        /// </summary>
        public IEnumerable<ICachedEntity> GetDirtyEntities(Type type, IEntityList entities)
        {
            // alternatively, one could consider creating an Attribute, which can be used to flag properties that should be synchronized automatically
            List<FieldInfo> field = ICachedEntity.GetCachedFieldInfos(type);

            return entities.Cast<ICachedEntity>().Where(entity => ICachedEntity.ArePropertiesDirty(entity, field)).ToList();
        }

        private List<ICachedEntity> PullFromContext<TCachedEntity>(DbContext context,
                                                            List<PullConflicts> conflicts,
                                                            bool force) where TCachedEntity : ICachedEntity =>
            PullFromContext(context, Mapper.Configuration.GetEntityMappingByCachedEntityType(typeof(TCachedEntity)), conflicts, force);

        private List<ICachedEntity> PullFromContext(DbContext context, IEntityCollectionMapping mapping, List<PullConflicts> conflicts, bool force)
        {
            var referencedCachedEntities = new List<ICachedEntity>();
            IEntityList repoCollection = mapping.GetRepositoryCollection(this);

            // we cache the timestamp before we access the database to make sure the timestamp is not newer
            // than any timestamp on any retrieved entity.
            var timeStamp = DateTime.UtcNow;

            // initialize our local repository collection with a copy of the database collection
            if (repoCollection == null)
            {
                // the local repository points to the collection pool, so creating a pool is sufficient to initialize the corresponding repository property.
                repoCollection =
                    Mapper.CachedEntityPool.CreatePool(mapping.TypeMapping.CachedEntityType).Collection;
                mapping.ForEachEntity(context, false, mapping.LastPullTime, entity =>
                {
                    ICachedEntity cachedEntity = mapping.TypeMapping
                           .CreateCachedEntityFromDefaultConstructor(entity
                               .Id);
                    repoCollection.AddEntity(cachedEntity);
                    referencedCachedEntities
                       .AddRange(Mapper.PullCachedEntityFromEntity(this, cachedEntity,
                            entity, mapping,
                            conflicts, force));
                });
            }
            else
            {
                mapping.ForEachEntity(context, true, mapping.LastPullTime, entity =>
                {
                    ICachedEntity cachedEntity = (ICachedEntity)repoCollection.GetEntity(entity.Id);
                    if (cachedEntity == null)
                    {
                        cachedEntity = mapping.TypeMapping
                                            .CreateCachedEntityFromDefaultConstructor(entity.Id);
                        repoCollection.AddEntity(cachedEntity);
                    }

                    // we need to pull only if the entity has never been loaded before (in which case the domain object was null above)
                    // or if it has been updated in the meantime
                    if (Mapper.DoesCachedEntityNeedUpdate(cachedEntity, entity))
                    {
                        referencedCachedEntities
                           .AddRange(Mapper.PullCachedEntityFromEntity(this, cachedEntity, entity,
                                         mapping, conflicts, force));
                    }
                });
            }

            mapping.LastPullTime = timeStamp;
            return referencedCachedEntities;
        }
    }
}
