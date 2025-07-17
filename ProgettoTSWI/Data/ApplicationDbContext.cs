using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using ProgettoTSWI.Models; // Cambialo con il tuo namespace reale

namespace ProgettoTSWI.Data
{
    public class ApplicationDbContext : DbContext
    {
       
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Participation> Participations { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Participation>()
                .HasOne(p => p.User)
                .WithMany(u => u.Participations)
                .HasForeignKey(p => p.ParticipationUserId);

            modelBuilder.Entity<Participation>()
                .HasOne(p => p.Event)
                .WithMany(e => e.Participations)
                .HasForeignKey(p => p.ParticipationEventId);


            modelBuilder.Entity<Event>()
                .HasOne(e => e.Organizer)
                .WithMany(u => u.OrganizedEvents)
                .HasForeignKey(e => e.OrganizerId)
                .OnDelete(DeleteBehavior.Restrict); // Previene la cancellazione cascata





        }
    }
}
//cd C:\Users\admin\source\repos\ProgettoTSWI_\ProgettoTSWI
