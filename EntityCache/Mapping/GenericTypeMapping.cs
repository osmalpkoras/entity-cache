using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
using EntityCache.Extensions;
using EntityCache.Interfaces;

namespace EntityCache.Mapping
{
    public class GenericTypeMapping<TCachedEntity, TEntity> : ITypeMapping
        where TCachedEntity : ICachedEntity where TEntity : IEntity
    {
        public GenericTypeMapping()
        {
            CachedEntityType = typeof(TCachedEntity);
            EntityType = typeof(TEntity);

            var properties = EntityType.GetProperties()
                                       .Where(info => info.GetCustomAttribute<NotMappedAttribute>() == null);

            foreach (var entityPropertyInfo in properties)
            {
                if (entityPropertyInfo.GetCustomAttribute<KeyAttribute>() != null)
                {
                    var cachedEntityPropertyInfo = CachedEntityType.GetProperty(entityPropertyInfo.Name);
                    IdMapping = new GenericPropertyMapping(cachedEntityPropertyInfo, entityPropertyInfo);
                }
                else
                {
                    var propertyMapping = CreatePropertyMapping(entityPropertyInfo);

                    // if we found a matching property in the domain object, we add it to the right set
                    if (propertyMapping != null)
                    {
                        if (entityPropertyInfo.PropertyType.IsAssignableTo<IEntity>())
                        {
                            ReferencedEntityPropertyMappings.Add(propertyMapping);
                        }
                        else
                        {
                            ValuePropertyMappings.Add(propertyMapping);
                        }
                    }
                }
            }
        }


        public Type CachedEntityType { get; protected set; }
        public Type EntityType { get; protected set; }

        public IPropertyMapping IdMapping { get; internal set; }

        /// <summary>
        ///     Property mappings used for properties of entity types.
        /// </summary>
        public List<IPropertyMapping> ReferencedEntityPropertyMappings { get; } = new List<IPropertyMapping>();

        /// <summary>
        ///     Property mappings for all properties, that are not of an entity type (they don't have their own table in the
        ///     database)
        /// </summary>
        public List<IPropertyMapping> ValuePropertyMappings { get; } = new List<IPropertyMapping>();

        public IEntity CreateEntityFromDefaultConstructor() =>
            (IEntity) CreateObjectFromDefaultConstructor(typeof(TEntity));

        public ICachedEntity CreateCachedEntityFromDefaultConstructor(string id)
        {
            var cachedEntity = (ICachedEntity) CreateObjectFromDefaultConstructor(typeof(TCachedEntity));
            cachedEntity.Id = id;
            return cachedEntity;
        }

        protected static object CreateObjectFromDefaultConstructor(Type type) =>
            // this gets the default constructor which has no parameters (doesn't work for constructors with default parameters)
            // an exception is thrown if the type does not have a public default constructor
            Activator.CreateInstance(type);

        /// <summary>
        ///     Returns the cached backing field on the domain object type, that matches the given source property info.
        ///     If the source property is named "SomeProperty", this will either return a cached backing field named
        ///     "_someProperty" on the given domain object type or null.
        /// </summary>
        public static FieldInfo GetCachedFieldForProperty(
            PropertyInfo sourcePropertyInfo,
            Type cachedEntityType)
        {
            // this returns the field named "_propertyName" for sourcePropertyInfo.Name = "PropertyName"
            var cachedFieldName =
                $"_{char.ToLowerInvariant(sourcePropertyInfo.Name[0])}{sourcePropertyInfo.Name[1..]}";
            var ret = cachedEntityType.GetField(cachedFieldName,
                                                ITypeMapping.PRIVATE_MEMBER_ACCESSOR_BINDING_ATTRIBUTES);
            ret = ret != null && ret.FieldType.IsAssignableTo<ICachedField>() ? ret : null;
            return ret;
        }

        private IPropertyMapping CreatePropertyMapping(PropertyInfo entityPropertyInfo)
        {
            IPropertyMapping propertyMapping = null;
            var cachedEntityFieldInfo = GetCachedFieldForProperty(entityPropertyInfo, CachedEntityType);
            if (cachedEntityFieldInfo != null)
            {
                propertyMapping = new CachedFieldMapping(cachedEntityFieldInfo, entityPropertyInfo);
            }
            else
            {
                var cachedEntityPropertyInfo = CachedEntityType.GetProperty(entityPropertyInfo.Name);
                if (cachedEntityPropertyInfo != null)
                {
                    propertyMapping = new GenericPropertyMapping(cachedEntityPropertyInfo, entityPropertyInfo);
                }
            }

            return propertyMapping;
        }
    }
}