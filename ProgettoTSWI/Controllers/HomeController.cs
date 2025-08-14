using System.Diagnostics;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net.Http;
using System.Text;

namespace ProgettoTSWI.Controllers
{
    public class UserDto
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string Ruolo { get; set; }
    }
    public class HomeController : Controller
    {
        private readonly IHttpClientFactory _httpClientFactory;
        public HomeController(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
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

        // Chiamata API per autenticazione utente
        [HttpPost]
        //[ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(string Email, string Password)
        {
            try
            {
                var client = _httpClientFactory.CreateClient();

                var loginData = new
                {
                    Email,
                    Password
                };

                var json = JsonConvert.SerializeObject(loginData);
                Console.WriteLine(json);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync("https://localhost:7087/api/HomeAPI/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseBody = await response.Content.ReadAsStringAsync();
                    var userInfo = JsonConvert.DeserializeObject<UserDto>(responseBody);

                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, userInfo.Id.ToString()),
                        new Claim(ClaimTypes.Role, userInfo.Ruolo)
                    };

                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

                    await HttpContext.SignInAsync(
                        CookieAuthenticationDefaults.AuthenticationScheme,
                        new ClaimsPrincipal(claimsIdentity)
                    );

                    if (userInfo.Ruolo == "Admin")
                    {
                        return RedirectToAction("Admin", "Home");
                    }
                    else //si assume che se un utente non è admin, è per forza user
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
            catch (Exception)
            {
                TempData["ErrorMessage"] = "Errore di comunicazione con l'API";
                return RedirectToAction("Index", "Home");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}


