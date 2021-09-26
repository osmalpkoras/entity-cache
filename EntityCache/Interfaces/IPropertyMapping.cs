using System;
using System.Reflection;

namespace EntityCache.Interfaces
{
    public interface IPropertyMapping
    {

        PropertyInfo EntityProperty { get; }
        Type EntityType { get; }
        Type CachedEntityType { get; }
        bool MapsToBackingField { get; }

        // if the destination property is writable, this writes to it
        void WriteToEntity(ICachedEntity cachedEntity, IEntity entity);

        void WriteToCachedEntity(ICachedEntity cachedEntity, IEntity entity);

        object GetValue(IEntity entity);

        void SetValue(IEntity entity, object value);

        object GetValue(ICachedEntity cachedEntity);
        void SetValue(ICachedEntity cachedEntity, object value);
    }
}
