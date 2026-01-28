using System.ComponentModel.DataAnnotations;

namespace GreenBook.Domain.Entities;

/// <summary>
/// Defines a hole for a specific tee set.
/// </summary>
public class CourseHole
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid TeeSetId { get; set; }

    public TeeSet TeeSet { get; set; } = null!;

    [Range(1, 18)]
    public int HoleNumber { get; set; }

    [Range(3, 5)]
    public int Par { get; set; }

    public int? Yardage { get; set; }

    [Range(1, 18)]
    public int? HandicapIndex { get; set; }
}
