using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.Security.Claims;
using System.Threading.Tasks;

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
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine("Errore di validazione: " + error.ErrorMessage);
            }
            return View(model);
        }


        // Controllo eventuale unicità email
        var existingUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Email già registrata");
            return View(model);
        }

        var user = new User
        {
            Name = model.Name,
            Surname = model.Surname,
            Aka = model.Aka,
            InstaProfile = model.InstaProfile,
            Email = model.Email,
            Password = BCrypt.Net.BCrypt.HashPassword(model.Password),
            //Ruolo = "User"
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();


        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.Email, user.Email),
            //di questo non sono del tutto sicuro perchè io non uso
            //nel database e nel controller in tipo Role ma semplicemente una stringa
            new Claim(ClaimTypes.Role, user.Ruolo)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

        // Reindirizza alla home dopo registrazione
        //return RedirectToAction("Index", "Home");

        //Reinderizza dopo la registrazione ad event
        return RedirectToAction("Index", "Event");
    }



}
