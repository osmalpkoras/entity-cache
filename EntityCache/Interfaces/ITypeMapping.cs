using System;
using System.Collections.Generic;
using System.Reflection;

namespace EntityCache.Interfaces
{
    public interface ITypeMapping
    {
        public const BindingFlags PRIVATE_MEMBER_ACCESSOR_BINDING_ATTRIBUTES =
            BindingFlags.Public
          | BindingFlags.NonPublic
          | BindingFlags.Instance
          | BindingFlags.Static
          | BindingFlags.FlattenHierarchy;

        Type CachedEntityType { get; }
        Type EntityType { get; }
        IPropertyMapping IdMapping { get; }

        /// <summary>
        ///     Property mappings used for properties of entity types.
        /// </summary>
        List<IPropertyMapping> ReferencedEntityPropertyMappings { get; }

        /// <summary>
        ///     Property mappings for all properties, that are not of an entity type (they don't have their own table in the
        ///     database)
        /// </summary>
        List<IPropertyMapping> ValuePropertyMappings { get; }

        IEntity CreateEntityFromDefaultConstructor();

        ICachedEntity CreateCachedEntityFromDefaultConstructor(string id);
    }
}