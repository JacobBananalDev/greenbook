using GreenBook.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GreenBook.Infrastructure.Persistence.Seeding
{
    public static class DatabaseSeeder
    {
        public static async Task SeedAsync(GreenBookDbContext db)
        {
            // ===============================
            // DEV USER (required for Rounds)
            // ===============================
            var devUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");

            if (!await db.Users.AnyAsync(u => u.Id == devUserId))
            {
                db.Users.Add(new User
                {
                    Id = devUserId,
                    Email = "dev@greenbook.app",
                    DisplayName = "Dev User",
                    CreatedAtUtc = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }

            // ===============================
            // DEV COURSE + TEESET + HOLES
            // ===============================
            // Use deterministic IDs so your POST payload is stable.
            var courseId = Guid.Parse("22222222-2222-2222-2222-222222222222");
            var teeSetId = Guid.Parse("33333333-3333-3333-3333-333333333333");

            // 1) Course
            var courseExists = await db.Courses.AnyAsync(c => c.Id == courseId);
            if (!courseExists)
            {
                db.Courses.Add(new Course
                {
                    Id = courseId,
                    Name = "Torrey Pines (South)",
                    City = "San Diego",
                    State = "CA",
                    Country = "USA",
                    CreatedAtUtc = DateTime.UtcNow,
                });

                await db.SaveChangesAsync();
            }

            // 2) TeeSet (must belong to Course)
            var teeSetExists = await db.TeeSets.AnyAsync(t => t.Id == teeSetId);
            if (!teeSetExists)
            {
                db.TeeSets.Add(new TeeSet
                {
                    Id = teeSetId,
                    CourseId = courseId,
                    Name = "Blue",
                    SlopeRating = 130,
                    CourseRating = 74.0m,
                    CreatedAtUtc = DateTime.UtcNow
                });

                await db.SaveChangesAsync();
            }

            // 3) 18 holes for that TeeSet (only if missing)
            var holesExist = await db.CourseHoles.AnyAsync(h => h.TeeSetId == teeSetId);
            if (!holesExist)
            {
                var holes = Enumerable.Range(1, 18).Select(holeNumber => new CourseHole
                {
                    Id = Guid.NewGuid(),
                    TeeSetId = teeSetId,
                    HoleNumber = holeNumber,
                    Par = (holeNumber % 5 == 0) ? 5 : (holeNumber % 3 == 0 ? 3 : 4),
                    Yardage = null,
                    HandicapIndex = null
                }).ToList();

                db.CourseHoles.AddRange(holes);
                await db.SaveChangesAsync();
            }

            Console.WriteLine("DatabaseSeeder: dev user/course/teeset/holes ensured.");
        }
    }
}
