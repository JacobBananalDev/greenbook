namespace GreenBook.Contracts.Courses;

public sealed record CreateCourseRequest(
    string Name,
    string? City,
    string? State,
    string? Country
);
