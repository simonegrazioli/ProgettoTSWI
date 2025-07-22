using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Linq.Expressions;


namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class DeleteReviewController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeleteReviewController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Back()
        {
            return View("../Home/Admin");
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int[] selectedReviews)
        {
            if (selectedReviews == null || selectedReviews.Length == 0)
            {
                TempData["ErrorMessage"] = "Nessuna reviews selezionata";
                return View("../Home/Admin");
            }

            try
            {

                //BISOGNA FARE UN UPDATE ALLA TABELLA PARTECIPATION AGGIORNANDO PartecipationReviews a "" 
                var reviewsToDelete = await _context.Participations.Where(p => selectedReviews.Contains(p.ParticipationId)).ToListAsync();

                foreach (var participation in reviewsToDelete)
                {
                    participation.ParticipationReview = string.Empty; // imposto a stringa vuota
                    _context.Entry(participation).Property(p => p.ParticipationReview).IsModified = true; // genererà: UPDATE Participations SET ParticipationReview = '' WHERE ParticipationId = X, dove X sono le review selezionate nella view
                }


                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Eliminate qta:{reviewsToDelete.Count} reviews con successo";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Si è verificato un errore durante l'eliminazione della recensione";
            }

            return View("../Home/Admin");
        }

    }
}
