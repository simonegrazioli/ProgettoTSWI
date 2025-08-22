using System;
using System.ComponentModel.DataAnnotations;

namespace ProgettoTSWI.Models
{
    public class EventCreation
    {
        [Required]
        [MaxLength(100)]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        [MaxLength(200)]
        public string EventLocation { get; set; }

        [MaxLength(1000)] // Opzionale: limita la lunghezza
        public string? Description { get; set; }

        public decimal? EventPrice { get; set; }

        // Aggiungi questo campo (non sarà visibile nel form)
        public int OrganizerId { get; set; } = 0;
    }
}