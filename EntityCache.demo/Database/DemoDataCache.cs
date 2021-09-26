using EntityCache.Caching;
using EntityCache.demo.DomainObjects;
using EntityCache.Interfaces;
using EntityCache.Pooling;

namespace EntityCache.demo.Database
{
    public class DemoDataCache : DataCache
    {
        public DemoDataCache(IMappingConfiguration configuration, IDataSource database) : base(configuration, database)
        {
        }

        public EntityList<Student> Students => Mapper.CachedEntityPool.GetPool<Student>();
        public EntityList<Lecturer> Lecturers => Mapper.CachedEntityPool.GetPool<Lecturer>();
        public EntityList<Lecture> Lectures => Mapper.CachedEntityPool.GetPool<Lecture>();
        public EntityList<Major> Majors => Mapper.CachedEntityPool.GetPool<Major>();
    }
}
