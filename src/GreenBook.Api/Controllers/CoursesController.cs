using GreenBook.Contracts.Courses;
using GreenBook.Domain.Entities;
using GreenBook.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GreenBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class CoursesController : ControllerBase
    {
        private readonly GreenBookDbContext _db;

        public CoursesController(GreenBookDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<CourseDto>>> GetAll(CancellationToken ct)
        {
            var courses = await _db.Courses
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new CourseDto(
                    c.Id,
                    c.Name,
                    c.City,
                    c.State,
                    c.Country
                ))
                .ToListAsync(ct);

            return Ok(courses);
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<CourseDto>> GetById(Guid id, CancellationToken ct)
        {
            var course = await _db.Courses
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CourseDto(
                    c.Id,
                    c.Name,
                    c.City,
                    c.State,
                    c.Country
                ))
                .FirstOrDefaultAsync(ct);

            return course is null ? NotFound() : Ok(course);
        }

        [HttpPost]
        public async Task<ActionResult<CourseDto>> Create([FromBody] CreateCourseRequest request, CancellationToken ct)
        {
            // Basic validation (keep it simple for now)
            var name = (request.Name ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(name))
                return BadRequest("Name is required.");

            if (name.Length > 140)
                return BadRequest("Name must be 140 characters or less.");

            // Create domain entity
            var course = new Course
            {
                Id = Guid.NewGuid(),
                Name = name,
                City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
                State = string.IsNullOrWhiteSpace(request.State) ? null : request.State.Trim(),
                Country = string.IsNullOrWhiteSpace(request.Country) ? null : request.Country.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Courses.Add(course);
            await _db.SaveChangesAsync(ct);

            var dto = new CourseDto(course.Id, course.Name, course.City, course.State, course.Country);

            // 201 + Location header pointing to GET by id
            return CreatedAtAction(nameof(GetById), new { id = course.Id }, dto);
        }
    }
}
