using System;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using EntityCache.Interfaces;

namespace EntityCache.demo.Entities
{
    [ExcludeFromCodeCoverage]
    public abstract class Entity : IEntity
    {
        [Key]
        public virtual string Id { get; set; }

        /// <summary>
        ///     True, when this entity has been marked as deleted.
        /// </summary>
        public virtual bool IsDeleted { get; set; }


        public DateTime UtcTimeStamp { get; set; }
    }
}
