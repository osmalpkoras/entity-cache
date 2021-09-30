using System;
using System.Collections.Generic;
using System.Linq;
using EntityCache.Caching;
using EntityCache.Exceptions;
using EntityCache.Interfaces;
using EntityCache.Pooling;
using Microsoft.EntityFrameworkCore;

namespace EntityCache.Mapping
{
    public class Mapper
    {
        public Mapper(IMappingConfiguration configuration)
        {
            Configuration = configuration;
        }

        public ObjectPool CachedEntityPool { get; } = new ObjectPool();

        public IMappingConfiguration Configuration { get; }


        public IEntity CreateEntityFromCachedEntity(DbContext context,
                                                    ICachedEntity cachedEntity,
                                                    ITypeMapping typeMapping,
                                                    DateTime newSavepoint,
                                                    ObjectPool newEntitiesPool)
        {
            var entity = typeMapping.CreateEntityFromDefaultConstructor();
            newEntitiesPool.Add(entity.GetType(), entity);

            var visitedEntities = new List<ICachedEntity>();
            CreateEntityFromCachedEntityRecursively(context,
                                                    cachedEntity,
                                                    entity,
                                                    typeMapping,
                                                    newSavepoint,
                                                    newEntitiesPool,
                                                    visitedEntities);
            return entity;
        }

        private void CreateEntityFromCachedEntityRecursively(DbContext context,
                                                             ICachedEntity cachedEntity,
                                                             IEntity entity,
                                                             ITypeMapping typeMapping,
                                                             DateTime newSavepoint,
                                                             ObjectPool newEntitiesPool,
                                                             List<ICachedEntity> visitedEntities)
        {
            if (cachedEntity == null || visitedEntities.Contains(cachedEntity))
            {
                return;
            }

            visitedEntities.Add(cachedEntity);

            foreach (var propertyMapping in typeMapping.ReferencedEntityPropertyMappings)
            {
                var referencedCachedEntity = (ICachedEntity) propertyMapping.GetValue(cachedEntity);
                IEntity referencedEntity = null;
                if (referencedCachedEntity != null)
                {
                    var typeMappingForReferencedCachedEntity
                        = Configuration.GetEntityMappingByCachedEntityType(referencedCachedEntity.GetType());
                    referencedEntity
                        = typeMappingForReferencedCachedEntity.FindEntity(context, referencedCachedEntity.Id);

                    // if we reference an object which already exists in the db, we will use the entity instance returned by EF and we are done
                    // otherwise the referenced domain object is new and we need to get a new matching entity
                    if (referencedEntity == null)
                    {
                        // if we have created it before already, we are done
                        var pooledEntity =
                            newEntitiesPool.Get(typeMappingForReferencedCachedEntity.TypeMapping.EntityType,
                                                referencedCachedEntity.Id);
                        // otherwise we need to create the entity
                        if (pooledEntity == null)
                        {
                            pooledEntity = typeMappingForReferencedCachedEntity.TypeMapping
                                                                               .CreateEntityFromDefaultConstructor();
                            newEntitiesPool.Add(pooledEntity.GetType(), pooledEntity);

                            CreateEntityFromCachedEntityRecursively(context,
                                                                    referencedCachedEntity,
                                                                    pooledEntity,
                                                                    typeMappingForReferencedCachedEntity.TypeMapping,
                                                                    newSavepoint,
                                                                    newEntitiesPool,
                                                                    visitedEntities);
                        }

                        referencedEntity = pooledEntity;
                    }
                }

                propertyMapping.SetValue(entity, referencedEntity);
            }

            foreach (var propertyMapping in typeMapping.ValuePropertyMappings)
            {
                propertyMapping.WriteToEntity(cachedEntity, entity);
            }

            entity.Id = cachedEntity.Id;
            entity.UtcTimeStamp = newSavepoint;
        }


        public IEntity CopyCachedEntityToEntity(DbContext context,
                                                ICachedEntity cachedEntity,
                                                IEntity entity,
                                                ITypeMapping typeMapping,
                                                DateTime newSavepoint,
                                                bool force)
        {
            if (entity.Id != cachedEntity.Id)
            {
                throw new EntityMismatchException(cachedEntity, entity);
            }

            // for entity properties, we load the referenced entity from the database.
            // the referenced entity must exist in the database!
            // to ensure this, all add operations should have been executed at this point.
            foreach (var propertyMapping in typeMapping.ReferencedEntityPropertyMappings)
            {
                var entityMappingForReferencedEntity
                    = Configuration.GetEntityMappingForPropertyMapping(propertyMapping);
                var referencedCachedEntity = (ICachedEntity) propertyMapping.GetValue(cachedEntity);
                IEntity dbEntity = null;
                if (referencedCachedEntity != null)
                {
                    dbEntity = entityMappingForReferencedEntity.FindEntity(context, referencedCachedEntity.Id);
                    if (dbEntity == null)
                    {
                        throw new UnexpectedReferenceToNonexistentEntityException(cachedEntity, referencedCachedEntity);
                    }
                }

                propertyMapping.SetValue(entity, dbEntity);
            }

            // otherwise we push properties which have backing fields to the database
            // if force is set to true, we will do this even for properties that have no backing field
            foreach (var propertyMapping in typeMapping.ValuePropertyMappings)
            {
                if (force || propertyMapping.MapsToBackingField)
                {
                    propertyMapping.WriteToEntity(cachedEntity, entity);
                }
            }

            entity.UtcTimeStamp = newSavepoint;
            return entity;
        }


        /// <returns>All domain objects which have not been pulled, but should be pulled. </returns>
        public List<ICachedEntity> PullCachedEntityFromEntity(
            IDataCache
                cache, // TODO: dieser Parameter soll weg, stattdessen sollen die pools dirent in der entity collection mapping landen
            ICachedEntity cachedEntity,
            IEntity entity,
            IEntityCollectionMapping entityMapping,
            List<PullConflicts> conflicts,
            bool force,
            bool recursive = false,
            DbContext context = null)
        {
            if (entity.Id != cachedEntity.Id)
            {
                throw new EntityMismatchException(cachedEntity, entity);
            }

            if (entity.IsDeleted)
            {
                // we do not pull a remotely deleted entity, because the last user to touch an entity is the user who deleted it
                // so we will not write data back anyway (thus we wont need to pull either)
                entityMapping.DeleteCachedEntity(cache, cachedEntity);
                // if any backing field of our domain object is dirty and the entity has been deleted, we have a conflict
                // the user needs to decide how to handle this situation. does the delete apply?
                if (cachedEntity.IsDirty)
                {
                    conflicts.Add(new PullConflicts {CachedEntity = cachedEntity, HasDeletionConflict = true});
                }

                return new List<ICachedEntity>();
            }

            var conflict = new PullConflicts {CachedEntity = cachedEntity};
            var referencedCachedEntities = new List<ICachedEntity>();

            foreach (var propertyMapping in entityMapping.TypeMapping.ReferencedEntityPropertyMappings)
            {
                PullCachedEntityPropertyFromEntityProperty(cache,
                                                           cachedEntity,
                                                           entity,
                                                           conflicts,
                                                           force,
                                                           recursive,
                                                           context,
                                                           propertyMapping,
                                                           referencedCachedEntities,
                                                           conflict);
            }

            foreach (var propertyMapping in entityMapping.TypeMapping.ValuePropertyMappings)
            {
                SetValueOnCachedEntity(cachedEntity,
                                       propertyMapping,
                                       propertyMapping.GetValue(entity),
                                       force,
                                       conflict);
            }

            // we handle the savepoint separately
            cachedEntity.UtcTimeStamp = entity.UtcTimeStamp;
            cachedEntity.IsNew = false;

            // if this domain objects has conflicts, we let the calling method know
            if (conflict.ConflictedProperties.Any())
            {
                conflicts.Add(conflict);
            }

            return referencedCachedEntities;
        }

        public void PullCachedEntityPropertyFromEntityProperty(
            IDataCache
                cache, // TODO: dieser Parameter soll weg, stattdessen sollen die pools dirent in der entity collection mapping landen
            ICachedEntity cachedEntity,
            IEntity entity,
            List<PullConflicts> conflicts,
            bool force,
            bool recursive,
            DbContext context,
            IPropertyMapping propertyMapping,
            List<ICachedEntity> referencedCachedEntities,
            PullConflicts conflict)
        {
            IEntity cachedEntityValue = null;
            var referencedEntity = (IEntity) propertyMapping.GetValue(entity);
            if (referencedEntity != null)
            {
                var entityMappingForReferencedEntity =
                    Configuration.GetEntityMappingForPropertyMapping(propertyMapping);
                // if the entity has been loaded as a domain object once already, we pull it from the pool

                cachedEntityValue =
                    CachedEntityPool.Get(entityMappingForReferencedEntity.TypeMapping.CachedEntityType,
                                         referencedEntity.Id);
                // if not, we need to create it here and now and add it to the pool
                if (cachedEntityValue == null)
                {
                    var referencedCachedEntity =
                        entityMappingForReferencedEntity.TypeMapping
                                                        .CreateCachedEntityFromDefaultConstructor(referencedEntity.Id);
                    // we add the referenced domain object to the pool in order to correctly resolve references in the future
                    CachedEntityPool.Add(entityMappingForReferencedEntity.TypeMapping.CachedEntityType,
                                         referencedCachedEntity);
                    // for new domain objects we also need to resolve referenced entities, this happens recursively
                    if (recursive)
                    {
                        // the recursion is stopped by adding the referencedCachedEntity to the CachedEntityPool above
                        PullCachedEntityAndReferencesFromContext(cache,
                                                                 context,
                                                                 referencedCachedEntity,
                                                                 entityMappingForReferencedEntity,
                                                                 conflicts,
                                                                 force);
                    }
                    // if we don't resolve references recursively, we need to return it as unresolved (this still needs to be pulled)
                    else
                    {
                        referencedCachedEntities.Add(referencedCachedEntity);
                    }

                    cachedEntityValue = referencedCachedEntity;
                }
            }

            SetValueOnCachedEntity(cachedEntity, propertyMapping, cachedEntityValue, force, conflict);
        }

        public void SetValueOnCachedEntity(ICachedEntity cachedEntity,
                                           IPropertyMapping propertyMapping,
                                           object cachedEntityValue,
                                           bool force,
                                           PullConflicts conflict)
        {
            if (force || propertyMapping.MapsToBackingField)
            {
                propertyMapping.SetValue(cachedEntity, cachedEntityValue);
                if (propertyMapping is CachedFieldMapping propertyFieldMapping)
                {
                    var backingField = propertyFieldMapping.GetPropertyAsCachedField(cachedEntity);
                    if (backingField.IsConflicted())
                    {
                        conflict.ConflictedProperties.Add((backingField, propertyFieldMapping.CachedEntityMember));
                    }
                }
            }
        }


        // TODO: IDataCache soll überall als parameter rauskommen, indem die objekt pools ausgelagert werden in die entity collection mappings, die in Configuration zu finden sind
        public void PullCachedEntityAndReferencesFromContext(
            IDataCache
                cache, // TODO: dieser Parameter soll weg, stattdessen sollen die pools dirent in der entity collection mapping landen.
            DbContext context,
            ICachedEntity cachedEntity,
            IEntityCollectionMapping mapping,
            List<PullConflicts> conflicts,
            bool force)
        {
            var entity = mapping.FindEntity(context, cachedEntity.Id);
            if (DoesCachedEntityNeedUpdate(cachedEntity, entity))
            {
                PullCachedEntityFromEntity(cache, cachedEntity, entity, mapping, conflicts, force, true, context);
            }
        }


        public bool DoesCachedEntityNeedUpdate(ICachedEntity cachedEntity, IEntity entity) =>
            entity.UtcTimeStamp > cachedEntity.UtcTimeStamp;
    }
}