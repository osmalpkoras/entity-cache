using EntityCache.demo.DomainObjects;
using EntityCache.demo.Entities;
using Microsoft.EntityFrameworkCore;

namespace EntityCache.demo.Database
{
    public class DemoDbContext : DbContext
    {
        public DemoDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<StudentEntity> Students { get; set; }
        public DbSet<Lecturer> Lecturers { get; set; }
        public DbSet<LectureEntity> Lectures { get; set; }
        public DbSet<Major> Majors { get; set; }
    }
}
