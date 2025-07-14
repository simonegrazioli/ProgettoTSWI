using Microsoft.EntityFrameworkCore;
using ProgettoTSWI.Models;

namespace ProgettoTSWI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<user> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<participation> Participations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<participation>()
                .HasOne(p => p.User)
                .WithMany(u => u.Participations)
                .HasForeignKey(p => p.ParticipationUserId);

            modelBuilder.Entity<participation>()
                .HasOne(p => p.Event)
                .WithMany(e => e.Participations)
                .HasForeignKey(p => p.ParticipationEventId);

        }
    }
}
