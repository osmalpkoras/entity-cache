using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;
using EntityCache.Interfaces;

namespace EntityCache.demo.DomainObjects
{
    public abstract class DomainObject : ICachedEntity
    {
        protected DomainObject()
        {
            Id = Guid.NewGuid().ToString("N");
        }

        [Key]
        public string Id { get; set; }

        /// <summary>
        ///     True, when this entity has been marked as deleted.
        /// </summary>
        public bool IsDeleted { get; set; }

        [NotMapped]
        public bool IsNew { get; set; } = true;

        [NotMapped]
        public bool IsDirty => ICachedEntity.ArePropertiesDirty(this, ICachedEntity.GetCachedFieldInfos(GetType()));

        [NotMapped]
        public bool IsConflicted => ICachedEntity.ArePropertiesConflicted(this);

        public DateTime UtcTimeStamp { get; set; }


        public void ForEachConflictingProperty(Action<ICachedField, FieldInfo> action)
        {
            List<FieldInfo> fields = ICachedEntity.GetCachedFieldInfos(GetType());

            // check if any property has a conflict and execute the given action on the conflicted properties
            foreach (FieldInfo fieldInfo in fields)
            {
                var field = (ICachedField)fieldInfo.GetValue(this);
                if (field.IsConflicted())
                {
                    action(field, fieldInfo);
                }
            }
        }
    }
}
