using System;
using System.Collections.Generic;
using System.Linq;
using EntityCache.Interfaces;

namespace EntityCache.Pooling
{
    public class ObjectPool
    {
        public ObjectPool()
        {
            Pool = new List<ITypePool>();
        }

        public List<ITypePool> Pool { get; set; }

        /// <summary>
        ///     Creates and returns a pool for the given type. If the pool exists already, it will be replaced.
        /// </summary>
        public TypePool<TObject> CreatePool<TObject>() where TObject : IEntity
        {
            var index = Pool.FindIndex(p => p.Type == typeof(TObject));
            if (index > -1)
            {
                Pool.RemoveAt(index);
            }

            var pool = new TypePool<TObject>();
            Pool.Add(pool);
            return pool;
        }

        public ITypePool CreatePool(Type type)
        {
            var index = Pool.FindIndex(p => p.Type == type);
            if (index > -1)
            {
                Pool.RemoveAt(index);
            }

            var genericTypePoolType = typeof(TypePool<>).MakeGenericType(type);
            var pool = (ITypePool) genericTypePoolType.GetConstructor(Type.EmptyTypes)?.Invoke(new object[] { });
            Pool.Add(pool);
            return pool;
        }

        /// <summary>
        ///     Returns the object pool for the given type or null if not created yet.
        /// </summary>
        public ITypePool GetPool(Type type)
        {
            return Pool.FirstOrDefault(tuple => tuple.Type == type);
        }

        public EntityList<TType> GetPool<TType>() where TType : IEntity
        {
            return ((TypePool<TType>) Pool.FirstOrDefault(p => p.Type == typeof(TType)))?.Objects;
        }

        /// <summary>
        ///     Returns the entity with the given id from the pool with the given type. Returns null if not found.
        /// </summary>
        public IEntity Get(Type type, string id)
        {
            var pool = Pool.SingleOrDefault(tuple => tuple.Type == type);
            return pool?.GetEntity(id);
        }

        /// <summary>
        ///     Adds the entity to the pool with the given type. If the pool doesn't exist yet, it will be created.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="obj"></param>
        public void Add(Type type, IEntity obj)
        {
            var pool = Pool.SingleOrDefault(tuple => tuple.Type == type);
            if (pool == null)
            {
                // we use the default (parameterless) constructor for creation of a new type.
                var poolType = typeof(TypePool<>).MakeGenericType(type);
                pool = (ITypePool) Activator.CreateInstance(poolType);
                Pool.Add(pool);
            }

            pool.Add(obj);
        }
    }
}