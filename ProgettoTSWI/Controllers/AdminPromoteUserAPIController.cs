
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using ProgettoTSWI.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using ProgettoTSWI.Models;
namespace ProgettoTSWI.Controllers
{

    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]

    public class AdminPromoteUserAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AdminPromoteUserAPIController(ApplicationDbContext context)
        {
            _context = context;
        }



        // Promuovo l'utente selezionato
        [HttpPost("Promote")]
        public async Task<IActionResult> PromoteUser([FromBody] idActionRequest request)
        {
            if (request.idSelected == null || request.idSelected.Length == 0)
            {
                return BadRequest(new { message = "Nessun utente selezionato." });
            }

            try
            {
                var userToPromote = await _context.Users.FirstOrDefaultAsync(us => request.idSelected.Contains(us.Id));

                if (userToPromote == null)
                {
                    return NotFound(new { message = "Utente non trovato." });
                }

                userToPromote.Ruolo = "Admin"; // Aggiorno la proprietà
                await _context.SaveChangesAsync();

                return Ok(new { message = "Utente promosso con successo." });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "Errore interno durante la promozione dell'utente" });
            }
        }        
    }
}
