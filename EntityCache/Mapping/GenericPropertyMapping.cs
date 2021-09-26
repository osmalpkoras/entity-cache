using System.Reflection;
using EntityCache.Interfaces;

namespace EntityCache.Mapping
{
    /// <summary>
    ///     This class maps a property from an entity to a property from a domain object.
    ///     It encapsulates writing and reading the property values for domain objects and entities
    ///     as well as copying the values from a domain object to an entity and vice verse.
    /// </summary>
    public class GenericPropertyMapping : PropertyMapping<PropertyInfo>
    {
        public GenericPropertyMapping(PropertyInfo cachedEntityMember, PropertyInfo entityProperty) : base(cachedEntityMember, entityProperty)
        {
            CachedEntityType = cachedEntityMember.PropertyType;
        }


        public override object GetValue(ICachedEntity cachedEntity)
        {
            return CachedEntityMember.GetValue(cachedEntity);
        }

        public override void SetValue(ICachedEntity cachedEntity, object value)
        {
            if (CachedEntityMember.CanWrite)
            {
                CachedEntityMember.SetValue(cachedEntity, value);
            }
        }
    }
}