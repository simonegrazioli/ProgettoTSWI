

using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using ProgettoTSWI.Models;
using System.Diagnostics;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Ruolo { get; set; }
}
public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }


    [AllowAnonymous]
    public IActionResult Login()

    {
        Console.WriteLine("************************-------QUi-----********************************************");
        return View("../Account/Login");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    
    // Mostra la pagina di Accesso Negato
    public IActionResult AccessDenied()
    {
        return View();
    }

    [Authorize(Roles = "Admin")] // Solo per admin
    public IActionResult Admin()
    {
        return View();
    }

    // Chiamata API per autenticazione utente
    [HttpPost]
    [ValidateAntiForgeryToken]
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

            var response = await client.PostAsync("https://localhost:7087/api/Api/login", content);

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
                    return View("~/Views/Home/Admin.cshtml");
                }
                else //si assume che se un utente non è admin, è per forza user
                {   
                    //qui devo aggiungere li altri claim
                    return RedirectToAction("Index", "Home");
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


    [HttpGet]
    public IActionResult Register()
    {
        return View(new User()); 
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(User model)
    {
        
        if (model.Password != model.ConfermaPassword)
        {
            ModelState.AddModelError("ConfermaPassword", "Le password non corrispondono");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            
            model.Email = model.Email?.Trim();
            model.Password = model.Password?.Trim();
            model.Name = model.Name?.Trim();
            model.Surname = model.Surname?.Trim();
            model.Aka = model.Aka?.Trim();


            var client = _httpClientFactory.CreateClient();
            //Qua poi lo cambio con quello di Simo ma con il doppio Hashing, cioe la doppia verifica
            var response = await client.PostAsJsonAsync("https://localhost:7087/api/Api/register", model);

            if (response.IsSuccessStatusCode)
            {
                var registeredUser = await response.Content.ReadFromJsonAsync<User>();

                var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, registeredUser.Id.ToString()),
                new(ClaimTypes.Email, registeredUser.Email),
                new(ClaimTypes.Name, registeredUser.Name),
                new(ClaimTypes.Role, registeredUser.Ruolo)
            };

                await HttpContext.SignInAsync(
                    new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", $"Errore durante la registrazione: {ex.Message}");
        }

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }
}