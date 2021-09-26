using System.Reflection;
using EntityCache.Interfaces;

namespace EntityCache.Mapping
{
    /// <summary>
    ///     This class maps a property from an entity to a cached field from a domain object.
    ///     It encapsulates writing and reading the property/field values for domain objects and entities
    ///     as well as copying the values from a domain object to an entity and vice verse.
    /// </summary>
    public class CachedFieldMapping : PropertyMapping<FieldInfo>
    {
        public CachedFieldMapping(FieldInfo cachedEntityMember, PropertyInfo entityProperty) : base(cachedEntityMember, entityProperty)
        {
            CachedEntityType = cachedEntityMember.FieldType.GetGenericArguments()[0];
            MapsToBackingField = true;
        }

        public ICachedField GetPropertyAsCachedField(ICachedEntity cachedEntity)
        {
            return (ICachedField)CachedEntityMember.GetValue(cachedEntity);
        }


        public override object GetValue(ICachedEntity cachedEntity)
        {
            return GetPropertyAsCachedField(cachedEntity).GetValueAsObject();
        }

        public override void SetValue(ICachedEntity cachedEntity, object value)
        {
            GetPropertyAsCachedField(cachedEntity).PullFromObject(value);
        }
    }
}