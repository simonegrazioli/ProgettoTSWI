//Yo!Simo, scusa se l'ho fatto solo ora ma in sti giorni sono stato pieno ahahh


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Security.Principal;

namespace ProgettoTSWI.Controllers
{

    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        [AllowAnonymous]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }


        [Authorize(Roles = "Admin")] // Solo per admin
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Roles = "User")] // Solo per user
        public IActionResult AfterLog()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            if (ModelState.IsValid)
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);

                if (user != null)
                {
                    // Verifica la password con BCrypt
                    if (BCrypt.Net.BCrypt.Verify(Password, user.Password))
                    {
                        // Login riuscito
                        var claims = new List<Claim>
                        {
                            new Claim(ClaimTypes.Role, user.Ruolo),
                            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                            new Claim(ClaimTypes.Name, user.Name),
                            new Claim(ClaimTypes.Email, user.Email)
                            //di questo non sono del tutto sicuro perchè io non uso
                            //nel database e nel controller in tipo Role ma semplicemente una stringa
                            //new Claim(ClaimTypes.Role, user.Ruolo)
                        };

                        var claimsIdentity = new ClaimsIdentity(
                            claims, CookieAuthenticationDefaults.AuthenticationScheme);

                        var principal = new ClaimsPrincipal(claimsIdentity);

                        await HttpContext.SignInAsync(
                            CookieAuthenticationDefaults.AuthenticationScheme,
                            new ClaimsPrincipal(principal));

                        // reindirizzamento basato sul ruolo
                        if (user.Ruolo == "Admin")
                        {
                            return RedirectToAction("Admin", "Home");
                        }
                        else //si assume che se un utente non � admin � per forza user
                        {
                            //Qui poi a livello teorico lo rimando sulla pagina degli eventi passando l'id
                            return RedirectToAction("Index", "Event");
                        }
                    }
                }
            }



            return RedirectToAction("Index", "Home"); //se fallisco il login vengo reindirizzato alla pagina di login
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Questo elimina il cookie e tutti i claims dell'utente
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            // Redirect alla home o alla pagina di login
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}

