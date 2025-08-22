using System.ComponentModel.DataAnnotations;

namespace ProgettoTSWI.Models
{
    public class ReviewModel
    {
        public int EventId { get; set; }
        public string EventName { get; set; }

        [Required(ErrorMessage = "Il commento è obbligatorio")]
        [StringLength(500, ErrorMessage = "Il commento non può superare i 500 caratteri")]
        public string Comment { get; set; }

        public int UserId { set; get; }
    }
}
