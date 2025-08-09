using Azure.Core;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using ProgettoTSWI.Models;
using System.Security.Claims;
using System.Text;
using System.Text.Json;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AccountController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    
    [HttpGet]
    public IActionResult Login()
    {
        return View(new LoginModel());
    }

    
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        try
        {
            var client = _httpClientFactory.CreateClient();
            
            var loginRequest = new
            {
                Email = model.Email?.Trim(), // Aggiungi Trim per sicurezza
                Password = model.Password?.Trim()
            };

           
            // 3. Invia con controllo degli headers
            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            var response = await client.PostAsync("https://localhost:7087/api/Api/login", content);
           

            if (response.IsSuccessStatusCode)
            {
                var userData = await response.Content.ReadFromJsonAsync<User>();

                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, userData.Id.ToString()),
                    new(ClaimTypes.Email, userData.Email),
                    new(ClaimTypes.Name, userData.Name),
                    new(ClaimTypes.Role, userData.Ruolo)
                };

                await HttpContext.SignInAsync(
                    new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)));

                

                return RedirectToAction("Index", "Home");
            }

            ModelState.AddModelError("", "Credenziali non valide");
        }
        catch
        {
            ModelState.AddModelError("", "Errore durante il login");
        }

        return View(model);
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