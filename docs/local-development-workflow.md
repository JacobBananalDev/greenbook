# GreenBook – Local Development Workflow

This document explains the **end‑to‑end local development workflow** for GreenBook.

It is written to help:

* New contributors get the project running quickly
* You (future‑you) remember *why* things are set up this way
* Reviewers understand that this project follows real‑world practices

---

## Overview (What You’re Doing)

GreenBook uses a **standard professional backend workflow**:

1. Domain models define the business rules
2. EF Core maps those models to a relational database
3. Migrations version database changes
4. Docker provides a reproducible local PostgreSQL environment

This mirrors how production systems are built — just scaled down for local dev.

---

## Tech Stack (Local Dev)

* **.NET 8 (LTS)** – Backend runtime
* **ASP.NET Core Web API** – API layer
* **EF Core** – ORM + migrations
* **PostgreSQL** – Relational database
* **Docker Desktop** – Local infrastructure

---

## Project Structure (Relevant Parts)

```
/greenbook
│
├─ docker-compose.yml        # Local Postgres
├─ /docs
│   └─ database-schema.md
|   └─ local-development-workflow.md
│
├─ /src
│   ├─ GreenBook.Api
│   ├─ GreenBook.Domain
│   │   └─ Entities
│   ├─ GreenBook.Infrastructure
│   │   └─ Persistence
│   │       ├─ GreenBookDbContext.cs
│   │       └─ Migrations
│   └─ GreenBook.Contracts
```

---

## Why Docker Is Used

Docker is used **only** for local infrastructure (PostgreSQL).

Benefits:

* No local DB installs
* Same setup on every machine
* Easy reset if something breaks
* Matches how teams collaborate

The application itself runs **natively**, not in a container.

---

## Docker Workflow

### Start the Database

From the repository root:

```powershell
docker compose up -d
```

This will:

* Start a PostgreSQL container
* Expose it on port `5432`
* Persist data using a named volume

Verify:

```powershell
docker ps
```

You should see `greenbook-db` running.

---

### Stop the Database

```powershell
docker compose down
```

Notes:

* This **does not delete data**
* Data is preserved via Docker volumes

---

### Reset the Database (Dangerous)

```powershell
docker compose down -v
```

⚠️ This **deletes all database data**.
Use only if you want a clean slate.

---

## EF Core Workflow

### Where Things Live

* **Entities** → `GreenBook.Domain`
* **DbContext + migrations** → `GreenBook.Infrastructure`
* **Config + startup** → `GreenBook.Api`

This separation is intentional and mirrors clean architecture.

---

### Create a Migration

From the `src` directory:

```powershell
dotnet ef migrations add InitialCreate \
  --project .\GreenBook.Infrastructure \
  --startup-project .\GreenBook.Api \
  --output-dir Persistence\Migrations
```

What this means:

* `--project` → where `DbContext` lives
* `--startup-project` → where config & DI live
* `--output-dir` → keeps migrations organized

---

### Apply the Migration

```powershell
dotnet ef database update \
  --project .\GreenBook.Infrastructure \
  --startup-project .\GreenBook.Api
```

This:

* Connects to Postgres
* Creates tables
* Applies constraints & indexes

---

### Verify the Database

```powershell
docker exec -it greenbook-db \
  psql -U postgres -d greenbook -c "\\dt"
```

You should see tables like:

* users
* courses
* tee_sets
* rounds
* round_holes

---

## Commit Strategy (Why Commits Are Split)

Each major concept is committed separately:

1. **Domain entities**
2. **Persistence / DbContext wiring**
3. **Docker Compose (infrastructure)**
4. **Initial migration**

This creates a readable project history and mirrors how teams work.

---

## Do Companies Expect This?

Yes — but not memorization.

What companies care about:

* You can run a project locally
* You understand DB + app wiring
* You know migrations exist and why
* You can debug environment issues

What they do **not** expect:

* Perfect recall of EF CLI flags
* Docker expertise on day one

This workflow signals:

> “I can work on real systems.”

---

## Next Improvements (Optional)

To make this even more professional:

* Add PowerShell scripts (`dev-up.ps1`, `dev-down.ps1`)
* Add README "Getting Started" section
* Add seed data for courses

---

## Summary

This workflow:

* Is industry‑standard
* Scales to real teams
* Is easy to onboard new developers

GreenBook is built like a real product — not a demo.
