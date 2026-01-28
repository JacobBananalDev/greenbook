using System.ComponentModel.DataAnnotations;

namespace GreenBook.Domain.Entities;

/// <summary>
/// Represents performance data for a single hole in a round.
/// </summary>
public class RoundHole
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid RoundId { get; set; }

    public Round Round { get; set; } = null!;

    [Range(1, 18)]
    public int HoleNumber { get; set; }

    public int Strokes { get; set; }
    public int? Putts { get; set; }
    public bool? Gir { get; set; }

    [MaxLength(10)]
    public string? FairwayResult { get; set; }

    public int? Penalties { get; set; }
    public int? SandShots { get; set; }
    public bool? UpAndDown { get; set; }
}
