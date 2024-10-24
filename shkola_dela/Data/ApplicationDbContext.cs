using Microsoft.EntityFrameworkCore;
using shkola_dela.Models;

public class ApplicationDbContext : DbContext
{
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is Client client)
            {
                if (entry.State == EntityState.Modified)
                {
                    client.DateUpdated = DateTime.UtcNow;
                }
            }
            if (entry.Entity is Founder founder)
            {
                if (entry.State == EntityState.Modified)
                {
                    founder.DateUpdated = DateTime.UtcNow;
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }
    
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    public DbSet<Client> Clients { get; set; }
    public DbSet<Founder> Founders { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>()
            .HasMany(c => c.Founders)
            .WithOne(f => f.Client)
            .HasForeignKey(f => f.ClientId);
    }
}


