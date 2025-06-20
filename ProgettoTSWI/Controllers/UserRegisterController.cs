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
                //aggiungo per verificare aka che sta dando problemi, me lo segna come null
                var akaErrors = ModelState["Aka"]?.Errors;
                if (akaErrors != null && akaErrors.Count > 0)
                {
                    foreach (var error in akaErrors)
                    {
                        // Logga o visualizza l'errore
                        Console.WriteLine("---------------da qui----------------");
                        Console.WriteLine(error.ErrorMessage);
                        Console.WriteLine("---------------a qui----------------");
                    }
                }


                return View(model);
            }

            var user = new User
            {
                Name = model.Name,
                Email = model.Email,
                Aka = model.Aka,
                Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
                //Ruolo = "Utente" // o "user"
                Ruolo = model.Ruolo,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            // TODO: invia mail di benvenuto
            // await _emailService.SendWelcomeEmail(user.Email);

            return RedirectToAction("Login");
        }
    }
}
