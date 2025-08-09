//For Metadata in APs.net
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <-- Serve perchè conferma password non è qualcosa che vado a salvare nel server
                                                    //devo segnarlo come non mappato

namespace ProgettoTSWI.Models
{
    public class User
    {
        public int Id { get; set; }  // chiave primaria con auto-incremento --> non va richiamata durante la creazione

        [Required(ErrorMessage = "Il nome è obbligatorio")]
        public string Name { get; set; }

        public string Surname { get; set; }

        public string? Aka { get; set; }
        public string? InstaProfile { get; set; }

        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserisci un'email valida")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La password è obbligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password", ErrorMessage = "Le password non corrispondono")]
        [NotMapped]
        public string ConfermaPassword { get; set; } = string.Empty; // Inizializza

        [Required] // Aggiungi Required se vuoi essere certo che non sia null
        public string Ruolo { get; set; } = "User";

        public virtual ICollection<Participation> Participations { get; set; } = new List<Participation>();

        public virtual ICollection<Event> OrganizedEvents { get; set; } = new List<Event>();



    }
}
