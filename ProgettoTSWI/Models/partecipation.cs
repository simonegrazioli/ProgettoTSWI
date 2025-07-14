using Microsoft.Extensions.Logging;

namespace ProgettoTSWI.Models
{
    public class participation
    {
        public int ParticipationId { get; set; }

        public string ParticipationReview { get; set; }

        // Foreign Keys
        public int ParticipationEventId { get; set; }
        public int ParticipationUserId { get; set; }
        public int ParticipationOrganizerId { get; set; }

        // Navigation Properties
        public virtual Event Event { get; set; }
        public virtual user User { get; set; }
    }

}
