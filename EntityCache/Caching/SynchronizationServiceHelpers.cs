using System.Collections.Generic;
using System.Linq;
using EntityCache.Extensions;

namespace EntityCache.Caching
{
    public static class SynchronizationServiceHelpers
    {
        public static void TakeLocalChanges(IEnumerable<PullConflicts> conflicts)
        {
            conflicts
                .Where(conflict => !conflict.HasDeletionConflict) // if entity has been deleted remotely, we do nothing
                .ForEach(conflict => conflict.ConflictedProperties.ForEach(conflictedProperty =>
                                                                           {
                                                                               conflictedProperty.CachedFields
                                                                                   .TakeLocal();
                                                                           }));
        }

        public static void DiscardLocalChanges(IEnumerable<PullConflicts> conflicts)
        {
            conflicts
                .Where(conflict => !conflict.HasDeletionConflict) // if entity has been deleted remotely, we do nothing
                .ForEach(conflict => conflict.ConflictedProperties.ForEach(conflictedProperty =>
                                                                           {
                                                                               conflictedProperty.CachedFields
                                                                                   .TakeSource();
                                                                           }));
        }
    }
}