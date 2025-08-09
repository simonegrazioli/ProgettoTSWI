using ProgettoTSWI.Models;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProgettoTSWI.Models
{ 
    public class Event
    {
        public int EventId { get; set; }

        [Required]
        [MaxLength(100)]
        public string EventName { get; set; }

        [MaxLength(1000)] // Opzionale: limita la lunghezza
        public string? Description { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public decimal? EventPrice { get; set; }

        [MaxLength(200)]
        public string EventLocation { get; set; }

        //aggiunta del campo approved per un evento
        public bool IsApproved { get; set; } = false;

        public int OrganizerId { get; set; }

        [NotMapped] // Questo attributo evita che EF provi a mappare questa proprietà al database
        public bool UserPartecipa { get; set; }

        public virtual User Organizer { get; set; }

        // Relazione con Participation
        public virtual ICollection<Participation> Participations { get; set; }


    }

}