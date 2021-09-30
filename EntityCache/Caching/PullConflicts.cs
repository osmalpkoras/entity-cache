using System.Collections.Generic;
using System.Reflection;
using EntityCache.Interfaces;

namespace EntityCache.Caching
{
    public class PullConflicts
    {
        public List<(ICachedField CachedFields, FieldInfo FieldInfo)> ConflictedProperties { get; set; } =
            new List<(ICachedField CachedFields, FieldInfo FieldInfo)>();

        public ICachedEntity CachedEntity { get; set; }

        /// <summary>
        ///     If this is true, we have local changes in RepositoryEntity but DatabaseEntity has been deleted.
        ///     RepositoryEntity will be deleted from the local cache and the user has to decide whether it should be readded.
        /// </summary>
        public bool HasDeletionConflict { get; set; }
    }
}