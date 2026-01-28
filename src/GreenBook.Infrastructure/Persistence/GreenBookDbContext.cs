using GreenBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenBook.Infrastructure.Persistence;

/// <summary>
/// Entity Framework Core DbContext for the GreenBook application.
///
/// Responsible for:
/// - Mapping domain entities to database tables
/// - Defining relationships, indexes, and constraints
/// - Acting as the persistence boundary (PostgreSQL) for the app
/// </summary>
public class GreenBookDbContext : DbContext
{
    public GreenBookDbContext(DbContextOptions<GreenBookDbContext> options)
        : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<Course> Courses => Set<Course>();
    public DbSet<TeeSet> TeeSets => Set<TeeSet>();
    public DbSet<CourseHole> CourseHoles => Set<CourseHole>();
    public DbSet<Round> Rounds => Set<Round>();
    public DbSet<RoundHole> RoundHoles => Set<RoundHole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Unique email
        modelBuilder.Entity<User>()
            .HasIndex(x => x.Email)
            .IsUnique();

        // TeeSet unique (CourseId, Name)
        modelBuilder.Entity<TeeSet>()
            .HasIndex(x => new { x.CourseId, x.Name })
            .IsUnique();

        // CourseHole unique (TeeSetId, HoleNumber)
        modelBuilder.Entity<CourseHole>()
            .HasIndex(x => new { x.TeeSetId, x.HoleNumber })
            .IsUnique();

        // RoundHole unique (RoundId, HoleNumber)
        modelBuilder.Entity<RoundHole>()
            .HasIndex(x => new { x.RoundId, x.HoleNumber })
            .IsUnique();

        // Check constraints
        modelBuilder.Entity<Round>()
            .ToTable(t =>
            {
                t.HasCheckConstraint("CK_rounds_holes_played", "\"holes_played\" IN (9, 18)");
                t.HasCheckConstraint("CK_rounds_starting_hole", "\"starting_hole\" IN (1, 10)");
            });
    }
}
