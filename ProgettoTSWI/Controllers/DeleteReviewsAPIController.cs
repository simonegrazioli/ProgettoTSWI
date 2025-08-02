using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Models;
using Microsoft.AspNetCore.Authorization;

namespace ProgettoTSWI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]

    public class DeleteReviewsAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DeleteReviewsAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPut("deleteReviews")]
        //[ValidateAntiForgeryToken] non va messo
        public async Task<IActionResult> DeleteReview([FromBody] idActionRequest request)
        {
            Console.WriteLine("richiesta "+request.idSelected);
            if (request.idSelected == null || request.idSelected.Length == 0)
            {
                return BadRequest(new { message = "Nessuna Reviews selezionata." });
            }

            try
            {

                //BISOGNA FARE UN UPDATE ALLA TABELLA PARTECIPATION AGGIORNANDO PartecipationReviews a "" 
                var previewsToDelete = await _context.Participations.Where(p => request.idSelected.Contains(p.ParticipationId)).ToListAsync();

                foreach (var participation in previewsToDelete)
                {
                    participation.ParticipationReview = string.Empty; // imposto a stringa vuota
                    _context.Entry(participation).Property(p => p.ParticipationReview).IsModified = true; // genererà: UPDATE Participations SET ParticipationReview = '' WHERE ParticipationId = X, dove X sono le review selezionate nella view
                }


                await _context.SaveChangesAsync();

                return Ok(new { message = $"{previewsToDelete.Count} reviews rimossa/e con successo." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Errore interno durante la cancellazione delle Reviews." });
            }
        }
    }
}
