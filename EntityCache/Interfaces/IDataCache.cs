using EntityCache.Caching;
using System;
using System.Collections.Generic;
using EntityCache.Mapping;

namespace EntityCache.Interfaces
{
    public interface IDataCache
    {
        void AddCachedEntity(ICachedEntity cachedEntity);

        public Mapper Mapper { get; }
        /// <summary>
        ///     The service that provides access to the database.
        /// </summary>
        IDataSource Database { get; }

        /// <summary>
        ///     Pulls all data from the database and updates the local cache to the latest savepoint.
        ///     Entities that had been removed remotely will be removed from the cache without checking for deletion conflicts.
        /// </summary>
        /// <param name="force">if true, properties of domain objects will be overwritten by entity properties when there is no backing field to handle the pulling/conflict.</param>
        /// <returns>a list of conflicts that were detected during a pull</returns>
        List<PullConflicts> Pull(bool force = false);

        /// <summary>
        ///     Pulls all data (of the given type, including referenced objects) from the database and updates the local cache.
        ///     Entities that had been removed remotely will be removed from the cache without checking for deletion conflicts.
        /// </summary>
        /// <param name="force">if true, properties of domain objects will be overwritten by entity properties when there is no backing field to handle the pulling/conflict.</param>
        /// <returns>a list of conflicts that were detected during a pull</returns>
        List<PullConflicts> Pull<TCachedEntity>(bool force = false) where TCachedEntity : ICachedEntity;

        /// <summary>
        ///     Pulls all data for the given domain object (and referenced objects) from the database and updates the local cache.
        ///     Entities that had been removed remotely will be removed from the cache without checking for deletion conflicts.
        /// </summary>
        /// <param name="cachedEntity">the domain object to be pulled from the database (all referenced entities will be pulled as well)</param>
        /// <param name="force">if true, properties of domain objects will be overwritten by entity properties when there is no backing field to handle the pulling/conflict.</param>
        /// <returns>a list of conflicts that were detected during a pull</returns>
        List<PullConflicts> Pull(ICachedEntity cachedEntity, bool force = false);

        /// <summary>
        ///     Pushes all local data to the database.
        /// </summary>
        /// <param name="force">When true, this will apply <see cref="ICachedField.TakeLocal"/> to all conflicts.</param>
        /// <returns></returns>
        bool Push(bool force = false);

        /// <summary>
        ///     Pushes all local data (of the given type) to the database.
        /// </summary>
        /// <param name="force">When true, this will apply <see cref="ICachedField.TakeLocal"/> to all conflicts.</param>
        /// <returns></returns>
        bool Push<TCachedEntity>(bool force = false) where TCachedEntity : ICachedEntity;

        /// <summary>
        ///     Pushes a domain object to the database.
        /// </summary>
        /// <param name="cachedEntity">the domain object to push to the database</param>
        /// <param name="force">When true, this will apply <see cref="ICachedField.TakeLocal"/> to all conflicts.</param>
        /// <returns></returns>
        bool Push(ICachedEntity cachedEntity, bool force = false);

        /// <summary>
        ///     Pulls all data from database, resolves conflicts and then pushes the local data to the database (with force = false)
        /// </summary>
        /// <param name="resolveConflictsCallback">the callback used to resolve conflicts</param>
        /// <returns></returns>
        bool Synchronize(Action<IEnumerable<PullConflicts>> resolveConflictsCallback);

        /// <summary>
        ///     Pulls all data (for the given type) from database, resolves conflicts and then pushes the local data (of the given type) to the database (with force = false)
        /// </summary>
        /// <param name="resolveConflictsCallback">the callback used to resolve conflicts</param>
        /// <returns></returns>
        bool Synchronize<TCachedEntity>(Action<IEnumerable<PullConflicts>> resolveConflictsCallback) where TCachedEntity : ICachedEntity;

        /// <summary>
        ///     Pulls the domain objects from database, resolves conflicts and then pushes the domain object to the database (with force = false)
        /// </summary>
        /// <param name="cachedEntity">the domain object to synchronize</param>
        /// <param name="resolveConflictsCallback">the callback used to resolve conflicts</param>
        /// <returns></returns>
        bool Synchronize(ICachedEntity cachedEntity, Action<IEnumerable<PullConflicts>> resolveConflictsCallback);
    }
}
