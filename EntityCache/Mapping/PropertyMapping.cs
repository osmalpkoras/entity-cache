using System;
using System.Diagnostics;
using System.Reflection;
using EntityCache.Interfaces;

namespace EntityCache.Mapping
{
    /// <summary>
    ///     The base class for all property mappings. A property mapping handles writing and reading data to and from a domain object or an entity.
    ///     It also provides convenient functionality to just copy the data of the mapped property from a domain object to an entity or vice versa.
    /// </summary>
    public abstract class PropertyMapping<TCachedEntityMemberType> : IPropertyMapping where TCachedEntityMemberType : MemberInfo
    {
        public PropertyInfo EntityProperty { get; protected set; }
        public Type EntityType { get; protected set; }
        public Type CachedEntityType { get; protected set; }
        public bool MapsToBackingField { get; protected set; }
        public TCachedEntityMemberType CachedEntityMember { get; protected set; }
        protected PropertyMapping(TCachedEntityMemberType cachedEntityMember, PropertyInfo entityProperty)
        {
            CachedEntityMember = cachedEntityMember;
            EntityProperty = entityProperty;
            EntityType = entityProperty.PropertyType;
        }

        // if the destination property is writable, this writes to it
        public void WriteToEntity(ICachedEntity cachedEntity, IEntity entity)
        {
            SetValue(entity, GetValue(cachedEntity));
        }

        public void WriteToCachedEntity(ICachedEntity cachedEntity, IEntity entity)
        {
            SetValue(cachedEntity, GetValue(entity));
        }

        public object GetValue(IEntity entity)
        {
            return EntityProperty.GetValue(entity);
        }

        public void SetValue(IEntity entity, object value)
        {
#if DEBUG
            if (!EntityProperty.CanWrite)
            {
                Debugger.Break(); // the property referred to by EntityProperty needs a setter!
            }
#endif
            EntityProperty.SetValue(entity, value);
        }

        public abstract object GetValue(ICachedEntity cachedEntity);
        public abstract void SetValue(ICachedEntity cachedEntity, object value);
    }
}