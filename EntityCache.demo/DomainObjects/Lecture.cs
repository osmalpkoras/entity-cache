using System.ComponentModel.DataAnnotations.Schema;
using EntityCache.demo.ValueTypes;

namespace EntityCache.demo.DomainObjects
{
    public class Lecture : DomainObject
    {
        public Lecturer Lecturer { get; set; }
        public Student Students { get; set; }
        public string Title { get; set; }
        public Major EligibleFor { get; set; }

        [NotMapped]
        public TimeSlot Time { get; set; }

        internal void Assign(Lecturer lecturer)
        {
            Lecturer = lecturer;
        }
    }
}
