# GreenBook — Database Schema (v1)

This document defines the v1 relational database schema for **GreenBook** and explains the reasoning behind key modeling choices.

Goals:
- Support **both 9-hole and 18-hole** rounds
- Store data in an **analytics-friendly** way (easy to query trends)
- Enforce **data integrity** via constraints (hard to break)
- Keep room for future features (handicap, strokes gained, shot tracking)

---

## Core Concepts (Mini Tutorial)

GreenBook stores two kinds of data:

1) **Course setup data (static)**
- Courses
- Tee sets (Blue/White/etc.)
- Hole definitions (par, yardage)

2) **Round performance data (dynamic)**
- A round played by a user
- Per-hole performance stats for that round (strokes, putts, GIR, etc.)

Why this matters:
- Static data is reused (a course doesn’t change every round)
- Dynamic data grows over time (your performance history)
- This structure makes analytics simple and avoids duplication

---

## Entities & Relationships

- **User** (1) → (many) **Rounds**
- **Course** (1) → (many) **TeeSets**
- **TeeSet** (1) → (many) **CourseHoles**
- **Course** (1) → (many) **Rounds**
- **Round** (1) → (many) **RoundHoles**
- **Round** references a **TeeSet** so scoring aligns with the correct par/yardage.

---

## ERD (Text View)

- User (1) —— (many) Round  
- Course (1) —— (many) TeeSet  
- TeeSet (1) —— (many) CourseHole  
- Course (1) —— (many) Round  
- Round (1) —— (many) RoundHole  

---

## Tables (v1)

### 1) users
Stores app users.

**Columns**
- `id` (uuid, PK)
- `email` (text, unique, required)
- `display_name` (text, nullable)
- `created_at_utc` (timestamp, required)

**Constraints**
- Unique: `email`

**Notes**
- Email uniquely identifies a user.
- Auth system can evolve later; this schema stays stable.

---

### 2) courses
Stores course metadata.

**Columns**
- `id` (uuid, PK)
- `name` (text, required)
- `city` (text, nullable)
- `state` (text, nullable)
- `country` (text, nullable)
- `created_at_utc` (timestamp, required)

**Notes**
- Courses are reusable across rounds and users.

---

### 3) tee_sets
Represents a tee option for a course (e.g., Blue/White/Black). Holds rating values for handicap/analytics.

**Columns**
- `id` (uuid, PK)
- `course_id` (uuid, FK → courses.id, required)
- `name` (text, required) — example: "Blue"
- `slope_rating` (int, nullable)
- `course_rating` (numeric, nullable)
- `created_at_utc` (timestamp, required)

**Constraints**
- Unique: `(course_id, name)` — a course can’t have two tee sets with the same name.

**Why this exists (Mini Tutorial)**
Hole yardage varies by tee set. If holes were attached directly to `courses`,
you’d have no clean way to store yardage differences for Blue vs White.

---

### 4) course_holes
Defines hole layout *per tee set*.

**Columns**
- `id` (uuid, PK)
- `tee_set_id` (uuid, FK → tee_sets.id, required)
- `hole_number` (int, required; 1–18)
- `par` (int, required; typically 3–5)
- `yardage` (int, nullable)
- `handicap_index` (int, nullable; 1–18)

**Constraints**
- Unique: `(tee_set_id, hole_number)` — tee set can only have one “hole 7”.
- Check: `hole_number BETWEEN 1 AND 18`

**Notes**
- `handicap_index` is optional; many courses publish it, but not required for v1.

---

### 5) rounds
One row per played round.

**Columns**
- `id` (uuid, PK)
- `user_id` (uuid, FK → users.id, required)
- `course_id` (uuid, FK → courses.id, required)
- `tee_set_id` (uuid, FK → tee_sets.id, required)
- `played_on` (date, required)
- `holes_played` (int, required; allowed: 9 or 18)
- `starting_hole` (int, nullable; default 1) — allowed: 1 or 10
- `notes` (text, nullable)
- `created_at_utc` (timestamp, required)

**Indexes**
- `(user_id, played_on DESC)` — fast “my rounds” history
- `(course_id)` — course analytics
- `(tee_set_id)` — tee analytics

**9-hole support (Design Rule)**
- `holes_played` decides if the round is 9 or 18 holes.
- `starting_hole` allows front 9 or back 9.

---

### 6) round_holes
One row per hole played in a round.

**Columns**
- `id` (uuid, PK)
- `round_id` (uuid, FK → rounds.id, required)
- `hole_number` (int, required; 1–18)
- `strokes` (int, required) — the score for that hole
- `putts` (int, nullable)
- `gir` (bool, nullable) — green in regulation
- `fairway_result` (text, nullable) — "Hit" | "Left" | "Right" | "NA"
- `penalties` (int, nullable)
- `sand_shots` (int, nullable)
- `up_and_down` (bool, nullable)
- `notes` (text, nullable)

**Constraints**
- Unique: `(round_id, hole_number)` — no duplicate holes in the same round
- Check: `hole_number BETWEEN 1 AND 18`

**9-hole support rule (v1)**
If `rounds.holes_played = 9`, the round should have 9 `round_holes` rows:
- If `starting_hole = 1`: holes 1–9
- If `starting_hole = 10`: holes 10–18

We’ll enforce this in application logic in v1.
(We can enforce it at the DB level later if needed.)

---

## Key Design Decisions (Mini Tutorial)

### Why split CourseHoles vs RoundHoles?
- `course_holes` = what the course *is* (par/yardage)
- `round_holes` = how you *performed* (strokes/putts/etc.)

This avoids duplication and keeps analytics accurate.

### Why not store total score on `rounds`?
Total score is derived: `SUM(round_holes.strokes)`.
Storing totals can get out of sync. We can add cached totals later if performance needs it.

---

## Future Extensions (v2+)

This model supports adding tables cleanly later:
- Shot tracking / strokes gained (`shots`)
- Weather conditions per round
- Tags (tournament/practice)
- Handicap snapshots
- Friends / shared rounds

---
