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

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }


    [ApiController]
    [Route("api/[controller]")]
    public class HomeAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public HomeAPIController(ApplicationDbContext context)
        {
            _context = context;
        }


            [HttpPost("login")]
            public async Task<IActionResult> Login([FromBody] LoginDto model)
            {
                if (!ModelState.IsValid)
                    return BadRequest(new { message = "Dati di login non validi." });

                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
                if (user == null || !BCrypt.Net.BCrypt.Verify(model.Password, user.Password))
                {
                    return Unauthorized(new { message = "Credenziali errate." });
                }

                // puoi anche restituire un token JWT se necessario
                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.Ruolo
                });
            }
    }

        

}


