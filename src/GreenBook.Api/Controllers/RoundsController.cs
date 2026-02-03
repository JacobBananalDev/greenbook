using GreenBook.Contracts.Rounds;
using GreenBook.Contracts.Rounds.RoundHoles;
using GreenBook.Domain.Entities;
using GreenBook.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;


namespace GreenBook.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public sealed class RoundsController : ControllerBase
    {
        private readonly GreenBookDbContext _db;

        public RoundsController(GreenBookDbContext db) => _db = db;

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<RoundDto>> GetById(Guid id, CancellationToken ct)
        {
            var round = await _db.Rounds
                .AsNoTracking()
                .Where(r => r.Id == id)
                .Select(r => new RoundDto(
                    r.Id,
                    r.UserId,
                    r.CourseId,
                    r.TeeSetId,
                    r.PlayedOn,
                    r.HolesPlayed,
                    r.StartingHole,
                    r.Notes,
                    r.CreatedAtUtc
                ))
                .FirstOrDefaultAsync(ct);

            return round is null ? NotFound() : Ok(round);
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<RoundDto>>> Get(
            [FromQuery] Guid? userId,
            CancellationToken ct)
        {
            var query = _db.Rounds.AsNoTracking();

            if (userId.HasValue)
                query = query.Where(r => r.UserId == userId.Value);

            var rounds = await query
                .OrderByDescending(r => r.PlayedOn)
                .Select(r => new RoundDto(
                    r.Id,
                    r.UserId,
                    r.CourseId,
                    r.TeeSetId,
                    r.PlayedOn,
                    r.HolesPlayed,
                    r.StartingHole,
                    r.Notes,
                    r.CreatedAtUtc
                ))
                .ToListAsync(ct);

            return Ok(rounds);
        }

        [HttpPost]
        public async Task<ActionResult<RoundDto>> Create([FromBody] CreateRoundRequest req, CancellationToken ct)
        {
            // Minimal validation (v1)
            if (req.HolesPlayed is not (9 or 18))
                return BadRequest("HolesPlayed must be 9 or 18.");

            if (req.StartingHole is not (1 or 10))
                return BadRequest("StartingHole must be 1 or 10.");

            // FK existence checks (nice quality for v1)
            var userExists = await _db.Users.AnyAsync(u => u.Id == req.UserId, ct);
            if (!userExists) return BadRequest("UserId does not exist.");

            var courseExists = await _db.Courses.AnyAsync(c => c.Id == req.CourseId, ct);
            if (!courseExists) return BadRequest("CourseId does not exist.");

            var teeExists = await _db.TeeSets.AnyAsync(t => t.Id == req.TeeSetId && t.CourseId == req.CourseId, ct);
            if (!teeExists) return BadRequest("TeeSetId does not exist for the given CourseId.");

            var round = new Round
            {
                Id = Guid.NewGuid(),
                UserId = req.UserId,
                CourseId = req.CourseId,
                TeeSetId = req.TeeSetId,
                PlayedOn = req.PlayedOn,
                HolesPlayed = req.HolesPlayed,
                StartingHole = req.StartingHole,
                Notes = string.IsNullOrWhiteSpace(req.Notes) ? null : req.Notes.Trim(),
                CreatedAtUtc = DateTime.UtcNow
            };

            _db.Rounds.Add(round);
            await _db.SaveChangesAsync(ct);

            var dto = new RoundDto(
                round.Id,
                round.UserId,
                round.CourseId,
                round.TeeSetId,
                round.PlayedOn,
                round.HolesPlayed,
                round.StartingHole,
                round.Notes,
                round.CreatedAtUtc
            );

            return CreatedAtAction(nameof(GetById), new { id = round.Id }, dto);
        }

        [HttpGet("{id:guid}/summary")]
        public async Task<ActionResult<RoundSummaryDto>> GetSummary(Guid id, CancellationToken ct)
        {
            var round = await _db.Rounds
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == id, ct);

            if (round is null)
                return NotFound("Round not found.");

            var holes = await _db.RoundHoles
                .AsNoTracking()
                .Where(h => h.RoundId == id)
                .ToListAsync(ct);

            if (holes.Count == 0)
                return Ok(new RoundSummaryDto(
                    id,
                    round.HolesPlayed,
                    0,
                    null,
                    0,
                    0
                ));

            var summary = new RoundSummaryDto(
                round.Id,
                round.HolesPlayed,
                holes.Sum(h => h.Strokes),
                holes.All(h => h.Putts is null) ? null : holes.Sum(h => h.Putts ?? 0),
                holes.Count(h => h.Gir == true),
                holes.Count(h => h.FairwayResult == "C")
            );

            return Ok(summary);
        }

        [HttpGet("{roundId:guid}/holes")]
        public async Task<ActionResult<List<RoundHoleDto>>> GetHoles(Guid roundId, CancellationToken ct)
        {
            // 1) Load round (need TeeSetId)
            var round = await _db.Rounds
                .AsNoTracking()
                .FirstOrDefaultAsync(r => r.Id == roundId, ct);

            if (round is null)
                return NotFound("Round not found.");

            // 2) Load tee-set holes (par, yardage, handicap, etc.)
            var courseHoles = await _db.CourseHoles
                .AsNoTracking()
                .Where(ch => ch.TeeSetId == round.TeeSetId)
                .OrderBy(ch => ch.HoleNumber)
                .ToListAsync(ct);

            // 3) Load any existing scoring rows
            var scored = await _db.RoundHoles
                .AsNoTracking()
                .Where(rh => rh.RoundId == roundId)
                .ToDictionaryAsync(rh => rh.HoleNumber, ct);

            // 4) Merge into a full scorecard list
            var result = courseHoles.Select(ch =>
            {
                scored.TryGetValue(ch.HoleNumber, out var rh);

                return new RoundHoleDto(
                    HoleNumber: ch.HoleNumber,
                    Par: ch.Par,
                    Strokes: rh?.Strokes,
                    Putts: rh?.Putts,
                    Gir: rh?.Gir,
                    FairwayResult: rh?.FairwayResult,
                    Penalties: rh?.Penalties,
                    SandShots: rh?.SandShots,
                    UpAndDown: rh?.UpAndDown,
                    UpdatedAtUtc: rh?.UpdatedAtUtc
                );
            }).ToList();

            return Ok(result);
        }


        [HttpPut("{roundId:guid}/holes/{holeNumber:int}")]
        public async Task<IActionResult> UpsertHole(
    Guid roundId,
    int holeNumber,
    [FromBody] UpsertRoundHoleRequest req,
    CancellationToken ct)
        {
            if (holeNumber < 1 || holeNumber > 18)
                return BadRequest("HoleNumber must be between 1 and 18.");

            var round = await _db.Rounds
                .FirstOrDefaultAsync(r => r.Id == roundId, ct);

            if (round is null)
                return NotFound("Round not found.");

            var hole = await _db.RoundHoles
                .FirstOrDefaultAsync(h =>
                    h.RoundId == roundId &&
                    h.HoleNumber == holeNumber, ct);

            if (hole is null)
            {
                hole = new RoundHole
                {
                    Id = Guid.NewGuid(),
                    RoundId = roundId,
                    HoleNumber = holeNumber,
                    CreatedAtUtc = DateTime.UtcNow
                };

                _db.RoundHoles.Add(hole);
            }

            hole.Strokes = req.Strokes;
            hole.Putts = req.Putts;
            hole.Gir = req.Gir;
            hole.FairwayResult = req.FairwayResult;
            hole.Penalties = req.Penalties;
            hole.SandShots = req.SandShots;
            hole.UpAndDown = req.UpAndDown;

            hole.UpdatedAtUtc = DateTime.UtcNow;

            await _db.SaveChangesAsync(ct);

            return NoContent();
        }

    }
}
