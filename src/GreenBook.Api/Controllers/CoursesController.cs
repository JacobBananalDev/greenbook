using GreenBook.Contracts.Courses;
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
    }
}
