using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamingStore.Contracts
{
    public class OpeningHours
    {
        public DayOfWeek DayOfWeek { get; set; }

        public string OpeningTime { get; set; }

        public string ClosingTime { get; set; }
    }
}
