namespace GreenBook.Contracts.Courses;

public sealed record CourseDto(
    Guid Id,
    string Name,
    string? City,
    string? State,
    string? Country
);
