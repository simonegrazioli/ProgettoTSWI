using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ProgettoTSWI.Models; // Cambialo con il tuo namespace reale

namespace ProgettoTSWI.Data
{
    namespace TuoProgetto.Data
    {
        public class ApplicationDbContext : DbContext
        {
            public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
                : base(options)
            {
            }

            public DbSet<User> Users { get; set; }
            // Aggiungi altri DbSet se hai altri modelli (es. Prenotazioni, Eventi, ecc.)
        }
    }

}

//cd C:\Users\admin\source\repos\ProgettoTSWI_\ProgettoTSWI
