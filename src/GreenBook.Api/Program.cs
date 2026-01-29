using GreenBook.Infrastructure.Persistence;
using GreenBook.Infrastructure.Persistence.Seeding;
using Microsoft.EntityFrameworkCore;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddDbContext<GreenBookDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("GreenBookDb")));

builder.Services.AddControllers();

// Swagger / OpenAPI (net8 standard)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

/* ===============================
   DEV-ONLY: DB migrate + seed
   =============================== */
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<GreenBookDbContext>();

    app.Logger.LogInformation("Running DB migrate + seed...");

    await db.Database.MigrateAsync();
    await DatabaseSeeder.SeedAsync(db);
}

/* ===============================
   HTTP request pipeline
   =============================== */
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
