using System.Collections.Generic;
using EntityCache.Interfaces;
using EntityCache.Pooling;

namespace EntityCache.Extensions
{
    public static class EntityListExtensions
    {
        public static EntityList<TEntity> ToEntityList<TEntity>(this IEnumerable<TEntity> source) where TEntity : IEntity
        {
            return new EntityList<TEntity>(source);
        }
    }
}
