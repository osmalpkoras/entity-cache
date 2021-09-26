namespace EntityCache.demo.DomainObjects
{
    public class Student : Person
    {
        public Major Major { get; set; }

        internal void Enroll(Major major)
        {
            Major = major;
        }

        internal void Enroll(Lecture computerAlgebra)
        {
            computerAlgebra.Students = this;
        }
    }
}
