using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using EntityCache.Interfaces;

namespace EntityCache.Exceptions
{
    /// <summary>
    ///     This exception is thrown whenever a entities with the same id were expected,
    ///     but at least one entity id does not match the id of another entity.
    /// </summary>
    [Serializable]
    public sealed class EntityMismatchException : Exception
    {
        public EntityMismatchException(params IEntity[] entities)
        {
            Entities = entities.ToList();
        }

        private EntityMismatchException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        ///     These are the entities that should have a matching id.
        /// </summary>
        public List<IEntity> Entities { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}