using GreenBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenBook.Infrastructure.Persistence.Seeding
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(GreenBookDbContext db)
        {
            if (await db.Courses.AnyAsync())
            {
                return;
            }

            // Course
            var course = new Course()
            {
                Id = Guid.NewGuid(),
                Name = "Torrey Pines (South)",
                City = "San Diego",
                State = "CA",
                Country = "USA",
                CreatedAtUtc = DateTime.UtcNow,
            };

            // Tee set
            var teeSet = new TeeSet
            {
                Id = Guid.NewGuid(),
                CourseId = course.Id,
                Name = "Blue",
                SlopeRating = 130,
                CourseRating = 74.0m,
                CreatedAtUtc = DateTime.UtcNow
            };

            // 18 holes (simple + realistic-ish defaults)
            var holes = Enumerable.Range(1, 18).Select(holeNumber => new CourseHole
            {
                Id = Guid.NewGuid(),
                TeeSetId = teeSet.Id,
                HoleNumber = holeNumber,
                Par = (holeNumber % 5 == 0) ? 5 : (holeNumber % 3 == 0 ? 3 : 4), // quick variety
                Yardage = null, // can fill later
                HandicapIndex = null
            }).ToList();

            db.Courses.Add(course);
            db.TeeSets.Add(teeSet);
            db.CourseHoles.AddRange(holes);

            Console.WriteLine("DatabaseSeeder: seeding starter data...");

            await db.SaveChangesAsync();
        }
    }
}
