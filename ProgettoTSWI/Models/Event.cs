using System.ComponentModel.DataAnnotations;

namespace ProgettoTSWI.Models
{
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public decimal? EventPrice { get; set; }

        [MaxLength(200)]
        public string EventLocation { get; set; }

        [MaxLength(50)]
        public string Status { get; set; }

        // Relazione con Participation
        public virtual ICollection<participation> Participations { get; set; }
    }

}
