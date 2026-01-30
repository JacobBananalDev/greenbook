namespace GreenBook.Contracts.Rounds;

public sealed record CreateRoundRequest(
    Guid UserId,
    Guid CourseId,
    Guid TeeSetId,
    DateOnly PlayedOn,
    int HolesPlayed,
    int StartingHole,
    string? Notes
);
