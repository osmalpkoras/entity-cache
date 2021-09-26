using System.ComponentModel.DataAnnotations.Schema;
using EntityCache.demo.DomainObjects;
using EntityCache.demo.ValueTypes;

namespace EntityCache.demo.Entities
{
    public class LectureEntity : DomainObject
    {
        public Lecturer Lecturer { get; set; }
        public StudentEntity Students { get; set; }
        public string Title { get; set; }
        public Major ElligibleFor { get; set; }

        [NotMapped]
        public TimeSlot Time { get; set; }

        internal void Assign(Lecturer lecturer)
        {
            Lecturer = lecturer;
        }
    }
}
