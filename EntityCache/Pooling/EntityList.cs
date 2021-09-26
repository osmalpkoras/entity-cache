using System.Collections;
using System.Collections.Generic;
using System.Linq;
using EntityCache.Interfaces;

namespace EntityCache.Pooling
{
    public class EntityList<TEntity> : List<TEntity>, IEntityList where TEntity : IEntity
    {
        public EntityList()
        {
        }

        public EntityList(IEnumerable<TEntity> collection) : base(collection)
        {
        }

        public EntityList(int capacity) : base(capacity)
        {
        }

        public List<TEntity> RemovedEntities { get; } = new List<TEntity>();

        public IEntity GetEntity(string id)
        {
            return this.FirstOrDefault(vo => vo.Id == id);
        }

        public IEnumerable GetRemovedEntities()
        {
            return RemovedEntities;
        }

        public void ClearRemovedEntities()
        {
            RemovedEntities.Clear();
        }

        public void AddEntity(TEntity entity)
        {
            if (!Contains(entity))
            {
                Add(entity);
            }
        }

        public void AddEntity(IEntity entity)
        {
            AddEntity((TEntity)entity);
        }

        public void AddEntities(IEnumerable<TEntity> collection)
        {
            foreach (TEntity entity in collection)
            {
                AddEntity(entity);
            }
        }

        public bool RemoveEntity(TEntity entity)
        {
            return RemoveEntity(entity?.Id);
        }

        public bool RemoveEntity(string id)
        {
            int entityIndex = FindIndex(e => e.Id == id);
            if (entityIndex > -1)
            {
                RemovedEntities.Add(this[entityIndex]);
                RemoveAt(entityIndex);
            }

            return true;
        }

        public TEntity UndoRemoveEntity(TEntity entity)
        {
            return UndoRemoveEntity(entity?.Id);
        }

        public TEntity UndoRemoveEntity(string id)
        {
            int entityIndex = RemovedEntities.FindIndex(e => e.Id == id);
            if (entityIndex > -1)
            {
                TEntity entity = RemovedEntities[entityIndex];
                Add(entity);
                RemovedEntities.RemoveAt(entityIndex);
                return entity;
            }

            return default;
        }

        public void RemoveAllEntities()
        {
            RemovedEntities.AddRange(this);
            Clear();
        }
    }
}
