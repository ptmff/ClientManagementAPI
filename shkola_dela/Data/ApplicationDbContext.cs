using Microsoft.EntityFrameworkCore;
using shkola_dela.Models;

public class ApplicationDbContext : DbContext
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Client client && entry.State == EntityState.Modified)
            {
                client.DateUpdated = DateTime.UtcNow;
            }
            if (entry.Entity is Founder founder && entry.State == EntityState.Modified)
            {
                founder.DateUpdated = DateTime.UtcNow;
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