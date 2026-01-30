namespace GreenBook.Contracts.Rounds;

public sealed record RoundSummaryDto(
    Guid RoundId,
    int HolesPlayed,
    int TotalStrokes,
    int? TotalPutts,
    int GirCount,
    int FairwaysHitCount
);
