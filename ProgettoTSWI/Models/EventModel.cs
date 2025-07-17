using ProgettoTSWI.Models;
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

        //aggiunta del campo approved per un evento
        public bool IsApproved { get; set; } = false;

        public int OrganizerId { get; set; }
        public virtual User Organizer { get; set; }

        // Relazione con Participation
        public virtual ICollection<Participation> Participations { get; set; }

        
    }

}