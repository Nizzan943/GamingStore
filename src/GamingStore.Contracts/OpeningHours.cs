using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GamingStore.Contracts
{
    public class OpeningHours
    {
        public DayOfWeek DayOfWeek { get; set; }

        [Required(ErrorMessage = "the Opening Time field is required.")]
        public string OpeningTime { get; set; }

        [Required(ErrorMessage = "the Closing Time field is required.")]
        public string ClosingTime { get; set; }
    }
}
