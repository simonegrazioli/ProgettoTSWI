using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Data;
using ProgettoTSWI.Models;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;


[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ApiController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ApiController> _logger;

    public ApiController(ApplicationDbContext context, ILogger<ApiController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuovo utente
    /// </summary>
    /// <param name="user">Dati utente</param>
    /// <returns>Dati utente registrato</returns>
    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register([FromBody] User user)
    {
        try
        {
            // Validazione
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            if (string.IsNullOrWhiteSpace(user.Password))
                return BadRequest("Password obbligatoria");

            // Controllo email esistente
            if (await _context.Users.AnyAsync(u => u.Email == user.Email))
                return Conflict("Email già registrata");


            user.Password = BCrypt.Net.BCrypt.HashPassword(user.Password);
            user.Ruolo ??= "User";

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Utente registrato: {user.Email}");
            return Ok(new { user.Id, user.Name, user.Email });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante la registrazione");
            return StatusCode(500, "Errore interno del server");
        }
    }

    public class LoginDto
    {
        public string Email { get; set; }
        public string Password { get; set; }
    }

    /// <summary>
    /// Effettua il login
    /// </summary>
    /// <param name="request">Credenziali di accesso</param>
    /// <returns>Dati utente autenticato</returns>
    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginDto model)
    {
        if (!ModelState.IsValid)
        {
            Console.WriteLine("Modello non valido");
            return BadRequest(new { message = "Dati di login non validi." });
        }


        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (user == null)
        {
            Console.WriteLine("Utente non trovato");
            return Unauthorized(new { message = "Credenziali errate." });
        }


        // Verifica 1: BCrypt standard
        bool isStandardValid = BCrypt.Net.BCrypt.Verify(model.Password, user.Password);

        // Verifica 2: BCrypt Enhanced
        bool isEnhancedValid = false;
        try
        {
            isEnhancedValid = BCrypt.Net.BCrypt.EnhancedVerify(model.Password, user.Password, HashType.SHA256);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore in EnhancedVerify: {ex.Message}");
        }

        if (!isStandardValid && !isEnhancedValid)
        {
            Console.WriteLine("Entrambe le verifiche hanno fallito");
            return Unauthorized(new { message = "Credenziali errate." });
        }

        // Se la password era in formato Enhanced, convertila in standard
        if (isEnhancedValid && !isStandardValid)
        {
            Console.WriteLine("Password valida ma in formato Enhanced, conversione in BCrypt standard...");
            user.Password = BCrypt.Net.BCrypt.HashPassword(model.Password);
            await _context.SaveChangesAsync();
            Console.WriteLine("Password convertita con successo");
        }

        return Ok(new { user.Id, user.Email, user.Ruolo });
    }




    /// <summary>
    /// Ritorna i dati di un utente
    ///METODO CON RICHIESTA DI AUTENTICAZIONE
    /// </summary>
    /// <param name="id">Id dell'utente</param>
    /// <returns>Dati utenteo</returns>
    [Authorize(Roles = "User")]
    [HttpGet("infoUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> infoUser(int id)
    {

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return BadRequest();
        }

        return Ok(user);
    }

    /// <summary>
    /// Lista eventi approvati
    /// </summary>
    /// <returns>Eventi approvati</returns>
    [HttpGet("events")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvents()
    {
        try
        {
            //cerco li eventi approvati
            var events = await _context.Events
                        .Where(e => e.IsApproved == true)
                        .ToListAsync();

            if (events == null || !events.Any())
            {
                return NotFound("Nessun evento trovato");
            }

            return Ok(events);
        }
        catch (Exception ex)
        {
            // Log dell'errore
            Console.WriteLine($"Errore durante il recupero degli eventi: {ex.Message}");
            return StatusCode(500, "Errore interno del server");
        }
    }

    /// <summary>
    /// Crea un nuovo evento. Li viene passato un'oggetto simile all'evento originale ma con meno attributi
    /// questo poi viene incorporato in un nuovo evento.
    ///METODO CON RICHIESTA DI AUTENTICAZIONE
    /// </summary>
    /// <param name="dto">Parte dell'evento che si creerà</param>
    /// <returns>Nuovo evento</returns>
    [Authorize(Roles = "User")]
    [HttpPost("createEvent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] EventCreation dto) // Accetta DTO invece di Entity
    {
        

        if (!ModelState.IsValid)
        {
           
            foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
            {
                Console.WriteLine(error.ErrorMessage);
            }
            return BadRequest(ModelState);
        }

        // Verifica duplicati
        bool eventExists = await _context.Events.AnyAsync(e =>
            e.EventName == dto.EventName &&
            e.EventLocation == dto.EventLocation &&
            e.EventDate == dto.EventDate);

        if (eventExists)
        {
            return BadRequest("Evento già esistente");
        }

        // Converti DTO in Entity
        var evento = new Event
        {
            EventName = dto.EventName,
            EventDate = dto.EventDate,
            EventLocation = dto.EventLocation,
            EventPrice = dto.EventPrice,
            OrganizerId = dto.OrganizerId,
            Description = dto.Description,
            IsApproved = false,
            Participations = new List<Participation>() // Inizializza la collezione
        };

        try
        {
            _context.Events.Add(evento);
            await _context.SaveChangesAsync();

            return Ok(evento); // Restituisci l'entity completa
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRORE durante il salvataggio: {ex.Message}");
            return StatusCode(500, "Errore interno del server");
        }
    }

    /// <summary>
    /// Lista di tutti gli eventi proposti da un utente, approvati e non
    ///METODO CON RICHIESTA DI AUTENTICAZIONE
    /// </summary>
    /// <param name="id">Id dell'utente</param>
    /// <returns>Lista di eventi</returns>
    [Authorize(Roles = "User")]
    [HttpGet("MyEvents")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> MyEvents(int id)
    {
        try
        {
            //Controllo se non cerca di accedere alle info di un'altro utente
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            int loggedUserId = int.Parse(userId);

            // Controlla che l'id passato coincida con quello dell'utente autenticato
            if (loggedUserId != id)
                return Forbid();
            
            var MyEvents = await _context.Events
                .Where(e => e.OrganizerId == id)  // CORRETTO: filtra gli eventi
                .ToListAsync();

            return Ok(MyEvents ?? new List<Event>());
        }
        catch (Exception ex)
        {
            // Log dell'errore
            Console.WriteLine($"Errore durante il recupero degli eventi: {ex.Message}");
            return StatusCode(500, "Errore interno del server");
        }
    }


    /// <summary>
    /// Informazioni di un evento in particolare
    /// </summary>
    /// <param name="idEvent">Id dell'evento</param>
    /// <returns>Informazioni dell'evento e i partecipanti</returns>
    [HttpGet("InfoEvent")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> InfoEvent(int idEvent)
    {
        try
        {
            var eventWithParticipants = await _context.Events
         .Where(e => e.EventId == idEvent)
         .Select(e => new EventWithParticipants
         {
             Event = e,
             ParticipantsAka = e.Participations
                 .Select(p => p.User.Aka)
                 .Where(aka => aka != null) // Filtra solo gli Aka non nulli
                 .ToList()
         })
         .FirstOrDefaultAsync();

            if (eventWithParticipants == null)
            {
                return NotFound();
            }

            return Ok(eventWithParticipants);
        }
        catch (Exception ex)
        {
            // Log dell'errore
            Console.WriteLine($"Errore durante il recupero degli eventi: {ex.Message}");
            return StatusCode(500, "Errore interno del server");
        }
    }

    /// <summary>
    /// Recupera tutte le recensioni di un evento e chi le ha fatte
    /// </summary>
    /// <param name="eventId">Id dell'evento</param>
    /// <returns>Recensioni, chi l'ha fatta</returns>
    [HttpGet("GetEventReviews")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetEventReviews(int eventId)
    {
        try
        {
            var reviews = await _context.Participations
                .Where(p => p.ParticipationEventId == eventId && p.ParticipationReview != null)
                .Join(_context.Users,
                    p => p.ParticipationUserId,
                    u => u.Id,
                    (p, u) => new { Participation = p, User = u })
                .Join(_context.Events,
                    pu => pu.Participation.ParticipationEventId,
                    e => e.EventId,
                    (pu, e) => new SeeReviewModel
                    {
                        EventName = e.EventName ?? "Evento sconosciuto",
                        Comment = pu.Participation.ParticipationReview ?? "Nessun commento",
                        UserName = pu.User.Name ?? "Anonimo",
                        UserAka = pu.User.Aka ?? "Aka: nessuno"
                    })
                .ToListAsync();

            return Ok(reviews);
        }
            catch (Exception) { 
                return BadRequest(); 
            }
        }

    /// <summary>
    /// Rimuove una partecipazione
    ///METODO CON RICHIESTA DI AUTENTICAZIONE
    /// </summary>
    /// <param name="request">Richiesta di rimozione</param>
    /// <returns>Ritorna 200 e salva il cambiamento</returns>
    [Authorize(Roles = "User")]
    [HttpPost("participations/delete")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteParticipation([FromBody] PartecipationConfirmModel request)
    {   int eventId = request.EventId; 
        int userId = request.UserId;
        

        try
        {
            var participation = await _context.Participations
                .FirstOrDefaultAsync(p =>
                    p.ParticipationEventId == eventId &&
                    p.ParticipationUserId == userId);

            if (participation == null)
            {
                Console.WriteLine("Partecipazione non trovata nel database");
                return NotFound();
            }

            
            _context.Participations.Remove(participation);
            int changes = await _context.SaveChangesAsync();


            return Ok();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERRORE durante l'operazione: {ex}");
            return StatusCode(500, "Errore interno");
        }
    }

    /// <summary>
    /// Guarda a cosa ha partecipato l'utente
    ///METODO CON RICHIESTA DI AUTENTICAZIONE
    /// </summary>
    /// <param name="userId">Id dell'utente</param>
    /// <returns>la lista delle partecipazioni</returns>
    [Authorize(Roles = "User")]
    [HttpGet("participations/user/{userId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetUserParticipations(string userId)
    {
        try
        {
            // Verifica se l'userId è valido
            if (string.IsNullOrEmpty(userId))
            {
                return BadRequest("UserId non valido");
            }

            // Converti userId in intero (se necessario)
            if (!int.TryParse(userId, out int userIdInt))
            {
                return BadRequest("UserId deve essere un numero intero");
            }

            // Cerca nel database tutte le partecipazioni dell'utente
            var partecipations = await _context.Participations
                .Where(p => p.ParticipationUserId == userIdInt)
                .ToListAsync();

            return Ok(partecipations);
        }
        catch (Exception ex)
        {
            return StatusCode(500, $"Errore interno del server: {ex.Message}");
        }
    }


    /// <summary>
    /// Piazza una revisione
    ///METODO CON RICHIESTA DI AUTENTICAZIONE
    /// </summary>
    /// <param name="model">modello della recensione</param>
    /// <returns>Aggiorna la recensione</returns>
    [Authorize(Roles = "User")]
    [HttpPut("participations/review")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateParticipationReview([FromBody] ReviewModel model)
    {
        // Validazione del modello
        if (!ModelState.IsValid)
        {
            Console.WriteLine("modello non valido");
            return BadRequest(ModelState);
        }
            

        var participation = await _context.Participations
            .FirstOrDefaultAsync(p => p.ParticipationEventId == model.EventId &&
                                    p.ParticipationUserId == model.UserId);

        if (participation == null)
            return NotFound();

        participation.ParticipationReview = model.Comment;
        await _context.SaveChangesAsync();

        return Ok();
    }


    /// <summary>
    /// Ritorna le informazioni di un evento
    /// </summary>
    /// <param name="id">Id dell'evento</param>
    /// <returns>Dati evento</returns>
    [HttpGet("events/{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetEvent(int id)
    {
        var evento = await _context.Events.FindAsync(id);
        if (evento == null)
            return NotFound();

        return Ok(evento);
    }


    /// <summary>
    /// Aggiunge la partecipazione di un certo User ad un evento
    ///METODO CON RICHIESTA DI AUTENTICAZIONE
    /// </summary>
    /// <returns>aggiunta partecipazione dell utente all'evento</returns>
    [Authorize(Roles = "User")]
    [HttpPost("participations/confirm")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> ConfirmParticipation([FromBody] PartecipationConfirmModel request)
    {
        
        try
        {
            // Verifica esistenza
            bool exists = await _context.Participations
                .AnyAsync(p => p.ParticipationEventId == request.EventId &&
                             p.ParticipationUserId == request.UserId);

            if (exists)
            {
                Console.WriteLine("Partecipazione già esistente");
                return Conflict("Partecipazione già esistente");
            }

            // Crea nuova partecipazione
            var participation = new Participation
            {
                ParticipationEventId = request.EventId,
                ParticipationUserId = request.UserId,
                
            };

            _context.Participations.Add(participation);
            await _context.SaveChangesAsync();

            Console.WriteLine($"Partecipazione salvata con ID: {participation.ParticipationId}");
            return Ok(new { Success = true, ParticipationId = participation.ParticipationId });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Errore grave: {ex}");
            return StatusCode(500, new { Error = "Errore interno del server" });
        }
    }

    /// <summary>
    /// Modifica le informazioni di un utente
    ///METODO CON RICHIESTA DI AUTENTICAZIONE
    /// </summary>
    /// <param name="dto">Una parte delle informazioni dell0 user, quelle modificabili</param>
    /// <returns>Dati utente modificati</returns>
    [Authorize(Roles = "User")]
    [HttpPost("EditUser")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> EditUser([FromBody] ModifyUser dto)
    {
        try
        {
            // Verifica che l'ID sia valido
            if (dto.Id <= 0)
            {
                return BadRequest("ID utente non valido");
            }

            // Cerca l'utente nel database
            var existingUser = await _context.Users.FindAsync(dto.Id);
            if (existingUser == null)
            {
                return NotFound("Utente non trovato");
            }

            // Aggiorna solo i campi che hanno valori (non null)
            if (!string.IsNullOrEmpty(dto.Name))
            {
                existingUser.Name = dto.Name;
            }

            if (!string.IsNullOrEmpty(dto.Surname))
            {
                existingUser.Surname = dto.Surname;
            }

            if (!string.IsNullOrEmpty(dto.Aka))
            {
                existingUser.Aka = dto.Aka;
            }

            if (!string.IsNullOrEmpty(dto.InstaProfile))
            {
                existingUser.InstaProfile = dto.InstaProfile;
            }

            if (!string.IsNullOrEmpty(dto.Email))
            {
                // Verifica che l'email sia valida
                if (new EmailAddressAttribute().IsValid(dto.Email))
                {
                    existingUser.Email = dto.Email;
                }
                else
                {
                    return BadRequest("Email non valida");
                }
            }

            if (!string.IsNullOrEmpty(dto.Password))
            {
                // Qui dovresti fare l'hash della password
                existingUser.Password = dto.Password;
            }

            // Salva le modifiche
            _context.Users.Update(existingUser);
            await _context.SaveChangesAsync();

            return Ok("Utente aggiornato con successo");
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }

}

    public class LoginRequest
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}