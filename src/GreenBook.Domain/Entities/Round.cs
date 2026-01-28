using System.ComponentModel.DataAnnotations;

namespace GreenBook.Domain.Entities;

/// <summary>
/// Represents a played golf round.
/// </summary>
public class Round
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required] public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    [Required] public Guid CourseId { get; set; }
    public Course Course { get; set; } = null!;

    [Required] public Guid TeeSetId { get; set; }
    public TeeSet TeeSet { get; set; } = null!;

    public DateOnly PlayedOn { get; set; }

    [Range(9, 18)]
    public int HolesPlayed { get; set; } = 18;

    [Range(1, 18)]
    public int StartingHole { get; set; } = 1;

    public string? Notes { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<RoundHole> Holes { get; set; } = new();
}
