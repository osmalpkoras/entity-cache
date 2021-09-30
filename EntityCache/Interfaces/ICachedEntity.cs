using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using EntityCache.Extensions;

namespace EntityCache.Interfaces
{
    public interface ICachedEntity : IEntity
    {
        public bool IsNew { get; set; }

        // soll als statisches Feld in jeder Klasse implementiert werden
        public bool IsDirty { get; }

        public bool IsConflicted { get; }

        void ForEachConflictingProperty(Action<ICachedField, FieldInfo> action);


        public static bool ArePropertiesDirty<TCachedEntity>(TCachedEntity cachedEntity, List<FieldInfo> fields)
            where TCachedEntity : ICachedEntity
        {
            if (cachedEntity != null && !cachedEntity.IsNew)
            {
                // check if properties of this entity are dirty
                return fields.Select(fieldInfo => (ICachedField) fieldInfo.GetValue(cachedEntity))
                             .Any(field => field.IsDirty());
            }

            return false;
        }

        public static bool ArePropertiesConflicted<TCachedEntity>(TCachedEntity cachedEntity)
            where TCachedEntity : ICachedEntity
        {
            if (cachedEntity != null)
            {
                var fields = GetCachedFieldInfos(cachedEntity.GetType());
                // check if properties of this entity are dirty
                return fields.Select(fieldInfo => (ICachedField) fieldInfo.GetValue(cachedEntity))
                             .Any(field => field.IsConflicted());
            }

            return false;
        }

        public static List<FieldInfo> GetCachedFieldInfos(Type entityType)
        {
            return entityType.GetFields(ITypeMapping.PRIVATE_MEMBER_ACCESSOR_BINDING_ATTRIBUTES)
                             .Where(info => info.FieldType.IsAssignableTo<ICachedField>())
                             .ToList();
        }
    }
}