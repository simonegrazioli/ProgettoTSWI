//For Metadata in APs.net
using System.ComponentModel.DataAnnotations;

namespace ProgettoTSWI.Models
{
    public class User
    {
        public int Id { get; set; }  // chiave primaria con auto-incremento --> non va richiamata durante la creazione

        [Required(ErrorMessage = "Il nome è obbligatorio")]
        public string Name { get; set; }

        public string Surname { get; set; }

        public string Aka { get; set; }
        public string InstaProfile { get; set; }

        [Required(ErrorMessage = "L'email è obbligatoria")]
        [EmailAddress(ErrorMessage = "Inserisci un'email valida")]
        public string Email { get; set; }

        [Required(ErrorMessage = "La password è obbligatoria")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Conferma la password")]
        [Compare("Password", ErrorMessage = "Le password non corrispondono")]
        [DataType(DataType.Password)]
        public string ConfermaPassword { get; set; }

        [Required(ErrorMessage ="Scegli il ruolo")]
        public string Ruolo { get; set; }


    }
}
