using Microsoft.Extensions.Logging;
using ProgettoTSWI.Models;

namespace ProgettoTSWI.Models
{
    public class Participation
    {
        public int ParticipationId { get; set; }

        //devo metterla nullable perchè un utente può premere sul pulsante per partecipare 
        // ma può non recensirla
        public string? ParticipationReview { get; set; }

        // Foreign Keys<
        public int ParticipationEventId { get; set; }
        public int ParticipationUserId { get; set; }
        

        // Navigation Properties
        public virtual Event Event { get; set; }
        public virtual User User { get; set; }
     
    }

}
