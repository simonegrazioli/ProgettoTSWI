

//Questo è un ibrido tra le due cose, cioè utilizza alcuni Unauthorized e BadRequest. Mentre per non 
// serializzare i dati quando li invio non uso ok() ma reinderizzo e basta che in teoria aprendo poi li strumenti con F12
// mi dovrebbe far vedere che il codice è 200


using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Diagnostics;
using System.Security.Claims;

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

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Roles = "User")]
        public IActionResult AfterLog()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Modello non valido.");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(Password, user.Password))
            {
                return Unauthorized("Credenziali non valide.");
            }

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Role, user.Ruolo),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Name),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(claimsIdentity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Redirect basato sul ruolo
            if (user.Ruolo == "Admin")
                return RedirectToAction("Admin", "Home");

            return RedirectToAction("Index", "Event");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Index", "Home");
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel
            {
                RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier
            });
        }
    }
}

