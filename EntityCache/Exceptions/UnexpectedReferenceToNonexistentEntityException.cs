using System;
using System.Runtime.Serialization;
using EntityCache.Interfaces;

namespace EntityCache.Exceptions
{
    /// <summary>
    ///     This exception is thrown whenever an entity is referencing another entity,
    ///     which is expected to exists, but couldn't be found.
    /// </summary>
    [Serializable]
    public sealed class UnexpectedReferenceToNonexistentEntityException : Exception
    {
        public UnexpectedReferenceToNonexistentEntityException(ICachedEntity cachedEntity,
                                                               ICachedEntity referencedCachedEntity)
        {
            CachedEntity = cachedEntity;
            ReferencedCachedEntity = referencedCachedEntity;
            EntityId = cachedEntity.Id;
            ReferencedEntityId = referencedCachedEntity.Id;
        }

        private UnexpectedReferenceToNonexistentEntityException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        /// <summary>
        ///     This is the domain object which holds a reference to a domain object, that has not been pushed to the database.
        /// </summary>
        public ICachedEntity CachedEntity { get; set; }

        /// <summary>
        ///     This is the referenced domain object, which has not been pushed to the database.
        /// </summary>
        public ICachedEntity ReferencedCachedEntity { get; set; }

        /// <summary>
        ///     This is the id of the entity which holds the unexpected reference to a non existent entity.
        /// </summary>
        public string EntityId { get; set; }

        /// <summary>
        ///     This is the id of the non existent entity, that has been referenced.
        /// </summary>
        public string ReferencedEntityId { get; set; }

        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            base.GetObjectData(info, context);
        }
    }
}