using System;

namespace EntityCache.Interfaces
{
    public interface IEntity
    {
        public string Id { get; set; }

        public DateTime UtcTimeStamp { get; set; }

        public bool IsDeleted { get; set; }
    }
}