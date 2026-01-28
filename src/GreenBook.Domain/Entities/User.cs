using System.ComponentModel.DataAnnotations;

namespace GreenBook.Domain.Entities;

/// <summary>
/// Represents an application user who can log golf rounds
/// </summary>
public class User
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(320)]
    public string Email { get; set; } = null!;

    [MaxLength(80)]
    public string? DisplayName { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    // Navigation property
    public List<Round> Rounds { get; set; } = new();
}
