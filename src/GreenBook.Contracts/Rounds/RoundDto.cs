namespace GreenBook.Contracts.Rounds;

public sealed record RoundDto(
    Guid Id,
    Guid UserId,
    Guid CourseId,
    Guid TeeSetId,
    DateOnly PlayedOn,
    int HolesPlayed,
    int StartingHole,
    string? Notes,
    DateTime CreatedAtUtc
);
