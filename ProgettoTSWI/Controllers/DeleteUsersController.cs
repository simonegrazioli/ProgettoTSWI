using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Linq.Expressions;


namespace ProgettoTSWI.Controllers
{
    public class DeleteUsersController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DeleteUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Back()
        {
            return View("../Home/Admin");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUsers(int[] selectedUsers)
        {
            if (selectedUsers == null || selectedUsers.Length == 0)
            {
                TempData["ErrorMessage"] = "Nessun utente selezionato";
                return View("../Home/Admin");
            }

            try
            {
                var usersToDelete = await _context.Users.Where(u => selectedUsers.Contains(u.Id)).ToListAsync();

                /*
                 
                TOGLI PARTECIPAZIONI
                 eventi proposti da lui ?
                 */


                _context.Users.RemoveRange(usersToDelete);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Eliminati n.{usersToDelete.Count} utenti con successo";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Si è verificato un errore durante l'eliminazione";
            }

            return View("../Home/Admin");
        }

    }
}
