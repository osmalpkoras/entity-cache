using System;

namespace EntityCache.Interfaces
{
    public interface ITypePool
    {
        /// <summary>
        ///     The type of every instance in this pool.
        /// </summary>
        Type Type { get; }

        /// <summary>
        ///     The generic reference to the underlying entity list collection, which contains all instances in this pool.
        /// </summary>
        IEntityList Collection { get; }

        /// <summary>
        ///     Returns the entity with the given id or null, if not contained in the pool.
        /// </summary>
        IEntity GetEntity(string id);

        /// <summary>
        ///     Adds the given entity.
        /// </summary>
        void Add(IEntity entity);

        /// <summary>
        ///     Removes the given entity.
        /// </summary>
        void Remove(IEntity entity);
    }
}