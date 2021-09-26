using EntityCache.Caching;
using EntityCache.demo.Database;
using EntityCache.Mapping;
using System;
using EntityCache.demo.DomainObjects;

namespace EntityCache.demo
{
    class Program
    {
        static void Main(string[] args)
        {
            Major mathsMaster = CreateMajor("Mathematics (Master of Science)");
            Major mathsBachelor = CreateMajor("Mathematics (Bachelor of Science)");
            Major computerScience = CreateMajor("Computer Science (Bachelor of Science)");

            Student student1 = CreateUniqueStudent("1");
            Student student2 = CreateUniqueStudent("2");

            Lecturer lecturer1 = CreateUniqueLecturer("1");
            Lecturer lecturer2 = CreateUniqueLecturer("2");

            Lecture computerAlgebra = CreateLecture("Computer Algebra");

            computerAlgebra.EligibleFor = mathsMaster;
            //computerAlgebra.EligibleFor.Add(mathsBachelor);
            //computerAlgebra.EligibleFor.Add(computerScience);
            computerAlgebra.Assign(lecturer1);

            //student1.Enroll(mathsMaster);
            //student1.Enroll(computerAlgebra);
            student2.Enroll(mathsBachelor);
            student2.Enroll(computerAlgebra);

            var configuration = new MappingConfiguration<DemoDataCache, DemoDbContext>();
            configuration
                .Map(db => db.Students, cache => cache.Students)
                .Map(db => db.Lecturers, cache => cache.Lecturers)
                .Map(db => db.Lectures, cache => cache.Lectures)
                .Map(db => db.Majors, cache => cache.Majors);

            var source = new AccessDatabase();
            var cache = new DemoDataCache(configuration, source);
            cache.AddCachedEntity(computerAlgebra);
            cache.Push(computerAlgebra, true);
            
            var source2 = new AccessDatabase();
            var cache2 = new DemoDataCache(configuration, source2);
            cache2.Pull();
        }

        private static Lecture CreateLecture(string identifier)
        {
            return new Lecture()
            {
                Title = identifier
            };
        }

        private static Major CreateMajor(string identifier)
        {
            return new Major()
            {
                Title = identifier
            };
        }

        private static Student CreateUniqueStudent(string identifier)
        {
            return new Student()
            {
                Name = $"Student {identifier}",
                Email = $"student.{identifier}@university.com",
                Address = $"Student Apartment Street {identifier}, 123456 University City"
            };
        }

        private static Lecturer CreateUniqueLecturer(string identifier)
        {
            return new Lecturer()
            {
                Name = $"Lecturer {identifier}",
                Email = $"Lecturer.{identifier}@university.com",
                Department = $"Personal {identifier} Department",
                Address = $"Lecturer Apartment Street {identifier}, 123456 University City"
            };
        }
    }
}
