using Microsoft.EntityFrameworkCore;
using Nsu.HackathonProblem.Models;

namespace Nsu.HackathonProblem.Database;

public class HackathonDbContext(DbContextOptions<HackathonDbContext> options)
    : DbContext(options)
{
    protected override void OnConfiguring(
        DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseNpgsql(
                "Host=localhost;Database=hackathon_db;Username=your_user;Password=your_password");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Ignore<Employee>();
        modelBuilder.Entity<EmployeeEntity>()
            .HasKey(e => new { e.Id, e.Role });
        modelBuilder.Entity<EmployeePreferenceEntity>().HasKey(e =>
            new { e.Role, e.EmployeeId, e.HackathonId, e.PreferredEmployeeId });
        modelBuilder.Entity<HackathonEntity>()
            .HasMany(h => h.Teams)
            .WithOne(t => t.Hackathon)
            .HasForeignKey(t => t.HackathonId)
            .OnDelete(DeleteBehavior.Cascade);

        base.OnModelCreating(modelBuilder);
    }


    public DbSet<HackathonEntity> Hackathons { get; set; }
    public DbSet<TeamEntity> Teams { get; set; }
    public DbSet<EmployeePreferenceEntity> EmployeePreferences { get; set; }
    public DbSet<EmployeeEntity> Employees { get; set; }
}