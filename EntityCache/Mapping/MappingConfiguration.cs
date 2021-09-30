using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using EntityCache.Interfaces;
using EntityCache.Pooling;
using Microsoft.EntityFrameworkCore;

namespace EntityCache.Mapping
{
    public class MappingConfiguration<TDataCache, TDbContext> : IMappingConfiguration
        where TDataCache : IDataCache
        where TDbContext : DbContext
    {
        public ITypeMapping GetTypeMappingByCachedEntityType(Type type)
        {
            return TypeMappings.First(m => m.CachedEntityType == type);
        }


        public IEntityCollectionMapping GetEntityMappingByCachedEntityType(Type type)
        {
            return EntityMappings.First(m => m.TypeMapping.CachedEntityType == type);
        }

        /// <summary>
        ///     Use this method if you want to retrieve a type mapping for an entity, as EF creates subclasses of entity types at
        ///     runtime.
        /// </summary>
        /// <param name="propertyMapping"></param>
        /// <returns></returns>
        public IEntityCollectionMapping GetEntityMappingForPropertyMapping(IPropertyMapping propertyMapping)
        {
        #if DEBUG
            if (EntityMappings.All(m => m.TypeMapping.EntityType != propertyMapping.EntityType))
            {
                //Logger.Log(fireLog.Level.Fatal,
                //           $"MISSING TYPE MAPPING: Tried to request a type mapping for entity of type {propertyMapping.EntityType.Name}.");
                Debugger.Break();
            }
        #endif
            return EntityMappings.First(m => m.TypeMapping.EntityType == propertyMapping.EntityType);
        }


        public List<ITypeMapping> TypeMappings { get; } = new List<ITypeMapping>();

        public List<IEntityCollectionMapping> EntityMappings { get; } = new List<IEntityCollectionMapping>();

        public MappingConfiguration<TDataCache, TDbContext> Map<TCachedEntity, TEntity>(
            Expression<Func<TDbContext, DbSet<TEntity>>> contextProperty,
            Expression<Func<TDataCache, EntityList<TCachedEntity>>>
                repositoryProperty)
            where TCachedEntity : ICachedEntity
            where TEntity : class, IEntity
        {
            // TODO: das muss noch ordentlich in die zugehörige funktion ausgelagert werden, sobald mehr creation-funktionen zur Verfüfung stehen
            var typeMapping = new GenericTypeMapping<TCachedEntity, TEntity>();
            TypeMappings.Add(typeMapping);
            EntityMappings.Add(new EntityCollectionMapping<TCachedEntity, TEntity>
            {
                TypeMapping = typeMapping,
                ContextProperty = ((MemberExpression) contextProperty.Body).Member as PropertyInfo,
                RepositoryProperty = ((MemberExpression) repositoryProperty.Body).Member as PropertyInfo
            });
            return this;
        }

        /// <summary>
        ///     Creates and returns type mapping for the given generic parameters. If it already exists, the existing mapping will
        ///     be returned.
        /// </summary>
        public MappingConfiguration<TDataCache, TDbContext> Map<TCachedEntity, TEntity>()
            where TCachedEntity : ICachedEntity where TEntity : IEntity
        {
            // TODO: hier muss noch ein Check rein damit nichts doppelt created wird.
            TypeMappings.Add(new GenericTypeMapping<TCachedEntity, TEntity>());
            return this;
        }

        /// <summary>
        ///     Creates and returns type mapping for the given generic parameters. If it already exists, the existing mapping will
        ///     be returned.
        /// </summary>
        public MappingConfiguration<TDataCache, TDbContext> Map<TCachedEntity>() where TCachedEntity : ICachedEntity
        {
            // TODO: hier muss noch ein Check rein damit nichts doppelt created wird.
            TypeMappings.Add(new GenericTypeMapping<TCachedEntity, TCachedEntity>());
            return this;
        }

        //public MappingOptions CreateGenericMapping<TDataCacheObject, TDataSourceObject>()
        //    where TDataCacheObject : class, IEntity
        //    where TDataSourceObject : class, IEntity
        //{
        //    return this;
        //}

        //// TODO: Ich brauche nicht zwischen domain und data object zu unterscheiden. ich kann auch einfach nur Entity : IEntity verwenden.
        ////       die Unterscheidung von Domain und Data object kann dann, wenn man möchte, selbst hinzufügen
        //public MappingOptions CreateGenericMapping<TDataCacheObject, TDataSourceObject>(Func<DataCache, EntityPool<TDataCacheObject>> p1, Func<XbrlDbContext, DbSet<TDataSourceObject>> p2)
        //    where TDataCacheObject : class, IEntity
        //    where TDataSourceObject : class, IEntity
        //{
        //    CreateGenericMapping<TDataCacheObject, TDataSourceObject>();
        //    return this;
        //}

        //public MappingOptions CreateMapping(ITypeMapping typeMapping)
        //{
        //    return this;
        //}

        //public MappingOptions CreateMapping<TDataCacheObject, TDataSourceObject>(Func<DataCache, EntityPool<TDataCacheObject>> dataCacheProperty, Func<XbrlDbContext, DbSet<TDataSourceObject>> dataSourceProperty)
        //    where TDataCacheObject : class, IEntity
        //    where TDataSourceObject : class, IEntity
        //{
        //    CreateGenericMapping<TDataCacheObject, TDataSourceObject>();
        //    return this;
        //}

        //public MappingOptions CreateMapping<TTypeMapping>() where TTypeMapping : class, ITypeMapping, new()
        //{
        //    return this;
        //}

        //public MappingOptions CreateMapping<TTypeMapping, TDataCacheObject, TDataSourceObject>(Func<DataCache, EntityPool<TDataCacheObject>> p1, Func<XbrlDbContext, DbSet<TDataSourceObject>> p2)
        //    where TTypeMapping : class, ITypeMapping, new()
        //    where TDataCacheObject : class, IEntity
        //    where TDataSourceObject : class, IEntity
        //{
        //    CreateMapping<TTypeMapping>();
        //    return this;
        //}
    }
}