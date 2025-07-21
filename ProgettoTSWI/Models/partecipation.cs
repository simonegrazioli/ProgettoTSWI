using Microsoft.Extensions.Logging;

namespace ProgettoTSWI.Models
{
    public class Participation
    {
        public int ParticipationId { get; set; }

        public string? ParticipationReview { get; set; }

        // Foreign Keys
        public int ParticipationEventId { get; set; }
        public int ParticipationUserId { get; set; }

        // Navigation Properties
        public virtual Event Event { get; set; }
        public virtual User User { get; set; }
    }

}
