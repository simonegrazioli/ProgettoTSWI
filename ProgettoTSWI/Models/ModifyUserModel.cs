//For Metadata in APs.net
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // <-- Serve perchè conferma password non è qualcosa che vado a salvare nel server
                                                    //devo segnarlo come non mappato

namespace ProgettoTSWI.Models
{
    public class ModifyUser
    {
        public int? Id { get; set; }
        public string? Name { get; set; }

        public string? Surname { get; set; }

        public string? Aka { get; set; }
        public string? InstaProfile { get; set; }

       
        [EmailAddress(ErrorMessage = "Inserisci un'email valida")]
        public string? Email { get; set; }

        
        [DataType(DataType.Password)]
        public string? Password { get; set; }


    }
}
