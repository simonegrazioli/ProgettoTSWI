using System;
using System.ComponentModel.DataAnnotations;

namespace ProgettoTSWI.Models
{
    public class EventFormViewModel
    {
        [Required]
        [MaxLength(100)]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [MaxLength(200)]
        public string EventLocation { get; set; }

        public decimal? EventPrice { get; set; }
    }
}
