using System.Collections;

namespace EntityCache.Interfaces
{
    // TODO: das soll IEntityList ebenfalls ersetzen
    public interface IEntityPool : IEnumerable
    {
    }

    //public class EntityPool<TType> : IEntityPool, IEnumerable<TType> where TType : class, IEntity
    //{
    //}
}