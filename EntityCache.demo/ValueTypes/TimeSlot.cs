using System;
using Microsoft.EntityFrameworkCore;

namespace EntityCache.demo.ValueTypes
{
    [Owned]
    public class TimeSlot
    {
        public DateTime Start { get; set; }
        public DateTime End { get; set; }
    }
}
