using System;
using System.Linq;
using EntityCache.Exceptions;
using EntityCache.Interfaces;

namespace EntityCache.Pooling
{
    public class TypePool<TType> : ITypePool where TType : IEntity
    {
        public TypePool()
        {
            Objects = new EntityList<TType>();
            Type = typeof(TType);
        }

        public Type Type { get; }
        public IEntityList Collection => Objects;

        public IEntity GetEntity(string id)
        {
            return Objects.SingleOrDefault(e => e.Id == id);
        }

        public void Add(IEntity entity)
        {
            TType obj = Objects.Find(o => o.Id == entity.Id);
            if (obj != null)
            {
                throw new EntityDuplicateException(obj, entity);
            }
            Objects.Add((TType)entity);
        }

        public void Remove(IEntity entity)
        {
            Objects.Remove((TType)entity);
        }

        public EntityList<TType> Objects { get; set; }
    }
}