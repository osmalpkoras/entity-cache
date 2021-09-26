using System.Collections;

namespace EntityCache.Interfaces
{
    public interface IEntityList : IEnumerable
    {
        void AddEntity(IEntity entity);

        bool RemoveEntity(string id);

        void RemoveAllEntities();

        IEntity GetEntity(string id);

        IEnumerable GetRemovedEntities();

        void ClearRemovedEntities();
    }
}