namespace GreenBook.Contracts.Rounds.RoundHoles;

public sealed record UpsertRoundHoleRequest(
    int Strokes,
    int? Putts,
    bool? Gir,
    string? FairwayResult,
    int? Penalties,
    int? SandShots,
    bool? UpAndDown
);