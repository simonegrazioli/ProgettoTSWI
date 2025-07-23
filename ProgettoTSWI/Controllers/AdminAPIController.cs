using Microsoft.AspNetCore.Mvc;
using System;
using Microsoft.AspNetCore.Authorization;
using ProgettoTSWI.Data;
//using System.Data.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
namespace ProgettoTSWI.Controllers
{


    [ApiController]
    [Route("api/[controller]")]
    public class AdminAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public AdminAPIController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Endpoint API che puoi vedere in Swagger
        [HttpGet("unapproved")]
        public async Task<IActionResult> GetUnapprovedEvents()
        {
            var proposedEvents = await _context.Events
                .Where(e => e.IsApproved == false)
                .ToListAsync();

            return Ok(proposedEvents);
        }

        [HttpGet("reviews")]
        public async Task<IActionResult> GetReviews() { 
            var allReviews= await _context.Participations.Where(p => p.ParticipationReview != "").ToListAsync();

            return Ok(allReviews);

        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers()
        {
            var allUsers =await  _context.Users.Where(u => u.Ruolo == "User").ToListAsync();

            return Ok(allUsers);
        }

        [HttpGet("confirmedEvents")]
        public async Task<IActionResult> GetConfirmedEvents()
        {
            var confirmedEvents = await _context.Events.Where(e => e.IsApproved == true).ToListAsync();

            return Ok(confirmedEvents);
        }
    }


}