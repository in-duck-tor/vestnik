using System.Reflection;
using InDuckTor.Vestnik.Domain;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Vestnik.Infrastructure.Database;

public class VestnikDbContext : DbContext
{
    public virtual DbSet<ClientAppRegistration> ClientAppRegistrations { get; set; }

    public VestnikDbContext(DbContextOptions<VestnikDbContext> options) : base(options)
    {
    }

    internal const string Schema = "vestnik";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}