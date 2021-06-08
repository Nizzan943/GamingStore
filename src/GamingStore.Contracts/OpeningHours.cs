using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamingStore.Contracts
{
    public class OpeningHours
    {
        public DayOfWeek DayOfWeek { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; }

        [DataType(DataType.Time)]
        public TimeSpan ClosingTime { get; set; }
    }
}
