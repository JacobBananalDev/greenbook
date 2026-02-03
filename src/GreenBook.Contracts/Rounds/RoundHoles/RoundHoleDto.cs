namespace GreenBook.Contracts.Rounds.RoundHoles;

public sealed record RoundHoleDto(
    int HoleNumber,
    int Par,
    int? Strokes,
    int? Putts,
    bool? Gir,
    string? FairwayResult,
    int? Penalties,
    int? SandShots,
    bool? UpAndDown,
    DateTime? UpdatedAtUtc
);
