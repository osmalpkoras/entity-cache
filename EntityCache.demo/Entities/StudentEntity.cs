using EntityCache.demo.DomainObjects;

namespace EntityCache.demo.Entities
{
    public class StudentEntity : Entity
    {
        public Major Major { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Address { get; set; }
    }
}
