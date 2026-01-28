using System.ComponentModel.DataAnnotations;

namespace GreenBook.Domain.Entities;

/// <summary>
/// Represents a golf course where rounds are played.
/// </summary>
public class Course
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(140)]
    public string Name { get; set; } = null!;

    [MaxLength(80)] public string? City { get; set; }
    [MaxLength(80)] public string? State { get; set; }
    [MaxLength(80)] public string? Country { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<TeeSet> TeeSets { get; set; } = new();
    public List<Round> Rounds { get; set; } = new();
}
