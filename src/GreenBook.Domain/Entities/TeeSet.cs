using System.ComponentModel.DataAnnotations;

namespace GreenBook.Domain.Entities;

/// <summary>
/// Represents a tee option for a course (e.g. Blue, White).
/// </summary>
public class TeeSet
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid CourseId { get; set; }

    public Course Course { get; set; } = null!;

    [Required, MaxLength(60)]
    public string Name { get; set; } = null!;

    public int? SlopeRating { get; set; }
    public decimal? CourseRating { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public List<CourseHole> Holes { get; set; } = new();
    public List<Round> Rounds { get; set; } = new();
}
