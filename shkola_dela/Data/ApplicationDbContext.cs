using Microsoft.EntityFrameworkCore;
using shkola_dela.Models;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

public class ApplicationDbContext : DbContext
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Client client && 
                (entry.State == EntityState.Modified || entry.State == EntityState.Added))
            {
                client.DateUpdated = DateTime.UtcNow;
            }
            else if (entry.Entity is Founder founder && 
                     (entry.State == EntityState.Modified || entry.State == EntityState.Added))
            {
                founder.DateUpdated = DateTime.UtcNow;
            }
            else if (entry.Entity is ClientFounder clientFounder &&
                     (entry.State == EntityState.Added || entry.State == EntityState.Deleted))
            {
                // Обновляем поле DateUpdated у связанного клиента
                var relatedClient = await Clients.FindAsync(clientFounder.ClientId);
                if (relatedClient != null)
                {
                    relatedClient.DateUpdated = DateTime.UtcNow;
                }

                // Обновляем поле DateUpdated у связанного учредителя
                var relatedFounder = await Founders.FindAsync(clientFounder.FounderId);
                if (relatedFounder != null)
                {
                    relatedFounder.DateUpdated = DateTime.UtcNow;
                }
            }
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Founder> Founders { get; set; }
    public DbSet<ClientFounder> ClientFounders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ClientFounder>()
            .HasKey(cf => new { cf.ClientId, cf.FounderId });

        modelBuilder.Entity<ClientFounder>()
            .HasOne(cf => cf.Client)
            .WithMany(c => c.ClientFounders)
            .HasForeignKey(cf => cf.ClientId);

        modelBuilder.Entity<ClientFounder>()
            .HasOne(cf => cf.Founder)
            .WithMany(f => f.ClientFounders)
            .HasForeignKey(cf => cf.FounderId);
    }
}
