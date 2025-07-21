using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Security.Claims;

namespace ProgettoTSWI.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: /User/Profile
        public async Task<IActionResult> Profile()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var user = await _context.Users
                .Include(u => u.Participations)
                    .ThenInclude(p => p.Event)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user == null) return NotFound();

            return View(user);
        }

        // GET: /User/Edit
        public async Task<IActionResult> Edit()
        {
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);
            return View(user);
        }

        // POST: /User/Edit
        [HttpPost]
        public async Task<IActionResult> Edit(User updatedUser)
        {
            
            var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var user = await _context.Users.FindAsync(userId);

            if (user == null) return NotFound();

            //in questo modo permetto di aggiornare anche solo 1 campo su 4 
            if (!string.IsNullOrWhiteSpace(updatedUser.Name))
                user.Name = updatedUser.Name;

            if (!string.IsNullOrWhiteSpace(updatedUser.Surname))
                user.Surname = updatedUser.Surname;

            if (!string.IsNullOrWhiteSpace(updatedUser.Email))
                user.Email = updatedUser.Email;

            if (!string.IsNullOrWhiteSpace(updatedUser.Name))
                user.Aka = updatedUser.Aka;

            if (!string.IsNullOrWhiteSpace(updatedUser.Email))
                user.InstaProfile = updatedUser.InstaProfile;

            

            await _context.SaveChangesAsync();
            TempData["Message"] = "Dati aggiornati correttamente!";
            return RedirectToAction("Profile");
        }
    }
}
