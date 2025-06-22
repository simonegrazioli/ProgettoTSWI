using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data.TuoProgetto.Data;
using ProgettoTSWI.Models;
using System.Threading.Tasks;

namespace ProgettoTSWI.Controllers
{
    public class UserRegisterController : Controller
    {
        private readonly ApplicationDbContext _context;

        public UserRegisterController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(User model)
        {
            if (!ModelState.IsValid)
            {
                
                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Surname = model.Surname,    
                Email = model.Email,
                InstaProfile = model.InstaProfile,
                Aka = model.Aka,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                Ruolo = model.Ruolo,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // TODO: invia mail di benvenuto
            // await _emailService.SendWelcomeEmail(user.Email);

            return RedirectToAction("Index", "Home");
        }
    }
}
