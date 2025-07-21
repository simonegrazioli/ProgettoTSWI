using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using Microsoft.EntityFrameworkCore;

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
            try
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
                                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                                new Claim(ClaimTypes.Role, user.Ruolo)
                            };

                            var claimsIdentity = new ClaimsIdentity(
                                claims, CookieAuthenticationDefaults.AuthenticationScheme);

                            await HttpContext.SignInAsync(
                                CookieAuthenticationDefaults.AuthenticationScheme,
                                new ClaimsPrincipal(claimsIdentity));

                            // reindirizzamento basato sul ruolo
                            if (user.Ruolo == "Admin")
                            {
                                return RedirectToAction("Admin", "Home");
                            }
                            else //si assume che se un utente non è admin è per forza user
                            {
                                return RedirectToAction("AfterLog", "Home");
                            }
                        }
                        else
                        {
                            TempData["ErrorMessage"] = "Login fallito, credenziali errate";
                            return RedirectToAction("Index", "Home");
                        }
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "Login fallito, credenziali errate";
                        return RedirectToAction("Index", "Home");
                    }
                }
            }catch(Exception ex)
            {
                TempData["ErrorMessage"] = "Problemi server";
                return RedirectToAction("Index", "Home");
            }



            return RedirectToAction("Index", "Home"); //se fallisco il login vengo reindirizzato alla pagina di login
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}


