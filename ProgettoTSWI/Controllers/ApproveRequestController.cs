using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;

namespace ProgettoTSWI.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ApproveRequestController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApproveRequestController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Back()
        {
            return View("../Home/Admin");
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmRefuseRequest(int[] selectedRequests, string actionType)
        {
            try
            {
                if(selectedRequests == null || selectedRequests.Count() == 0)
                {
                    TempData["ErrorMessage"] = "Nessun evento selezionato da confermare";
                    return View("../Home/Admin");
                }
                if (actionType == "approve")
                {
                    var eventToConfirm = await _context.Events.Where(e => selectedRequests.Contains(e.EventId)).ToListAsync();

                    foreach (var ev in eventToConfirm)
                    {
                        ev.IsApproved = true;
                        _context.Entry(ev).Property(e => e.IsApproved).IsModified = true;

                        // creo la partecipazione tra l'organizzatore e l'evento
                        var participation = new Participation
                        {
                            ParticipationEventId = ev.EventId,
                            ParticipationUserId = ev.OrganizerId
                        };

                        _context.Participations.Add(participation);
                    }

                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Confermate qta:{eventToConfirm.Count} eventi con successo";
                }
                else if (actionType == "refuse")
                {
                    var eventToRefuse = await _context.Events.Where(ev => selectedRequests.Contains(ev.EventId)).ToListAsync();

                    _context.Events.RemoveRange(eventToRefuse);
                    await _context.SaveChangesAsync();

                    TempData["SuccessMessage"] = $"Rifiutati qta:{eventToRefuse.Count} eventi con successo";

                }


            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Si è verificato un errore durante l'approvazione/rifiuto degli eventi";
            }

            return View("../Home/Admin");
        }
    }
}
