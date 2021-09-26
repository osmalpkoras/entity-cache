using System;
using System.Runtime.Serialization;
using EntityCache.Interfaces;

namespace EntityCache.Exceptions
{
    /// <summary>
    ///     This exception is thrown whenever a duplicate entity is unexpectedly detected.
    ///     Duplication is dependent on the equality of the entity IDs, not their values.
    /// </summary>
    [Serializable]
    public sealed class EntityDuplicateException : Exception
    {
        /// <summary>
        ///     This is the already existing entity.
        /// </summary>
        public IEntity ExistingEntity { get; set; }

        /// <summary>
        ///     This is the entity duplicate
        /// </summary>
        public IEntity DuplicateEntity { get; set; }

        public EntityDuplicateException(IEntity existingEntity, IEntity duplicateEntity)
        {
            ExistingEntity = existingEntity;
            DuplicateEntity = duplicateEntity;
        }

        private EntityDuplicateException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        { }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}
