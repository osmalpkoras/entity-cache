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

        public EntityList(IEnumerable<TEntity> collection)
            : base(collection)
        {
        }

        public EntityList(int capacity)
            : base(capacity)
        {
        }

        public List<TEntity> RemovedEntities { get; } = new List<TEntity>();

        public IEntity GetEntity(string id)
        {
            return this.FirstOrDefault(vo => vo.Id == id);
        }

        public IEnumerable GetRemovedEntities() => RemovedEntities;

        public void ClearRemovedEntities()
        {
            RemovedEntities.Clear();
        }

        public void AddEntity(IEntity entity)
        {
            AddEntity((TEntity) entity);
        }

        public bool RemoveEntity(string id)
        {
            var entityIndex = FindIndex(e => e.Id == id);
            if (entityIndex > -1)
            {
                RemovedEntities.Add(this[entityIndex]);
                RemoveAt(entityIndex);
            }

            return true;
        }

        public void RemoveAllEntities()
        {
            RemovedEntities.AddRange(this);
            Clear();
        }

        public void AddEntity(TEntity entity)
        {
            if (!Contains(entity))
            {
                Add(entity);
            }
        }

        public void AddEntities(IEnumerable<TEntity> collection)
        {
            foreach (var entity in collection)
            {
                AddEntity(entity);
            }
        }

        public bool RemoveEntity(TEntity entity) => RemoveEntity(entity?.Id);

        public TEntity UndoRemoveEntity(TEntity entity) => UndoRemoveEntity(entity?.Id);

        public TEntity UndoRemoveEntity(string id)
        {
            var entityIndex = RemovedEntities.FindIndex(e => e.Id == id);
            if (entityIndex > -1)
            {
                var entity = RemovedEntities[entityIndex];
                Add(entity);
                RemovedEntities.RemoveAt(entityIndex);
                return entity;
            }

            return default;
        }
    }
}