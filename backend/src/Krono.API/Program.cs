using System.Text.Json.Serialization;
using Krono.Application.Services;
using Krono.Domain.Interfaces;
using Krono.Domain.Services;
using Krono.Domain.Services.Strategies;
using Krono.Infrastructure.Persistence;
using Krono.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Controllers & JSON configuration
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS: Allow Frontend interaction
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Database Setup: Check if PostgreSQL is reachable, otherwise use InMemory
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
bool usePostgres = false;

if (!string.IsNullOrEmpty(connectionString))
{
    try
    {
        using var conn = new NpgsqlConnection(connectionString);
        conn.Open();
        usePostgres = true;
    }
    catch
    {
        usePostgres = false;
    }
}

if (usePostgres)
{
    builder.Services.AddDbContext<KronoDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    builder.Services.AddDbContext<KronoDbContext>(options =>
        options.UseInMemoryDatabase("KronoDb"));
}

// Domain & Application Services Registration (SOLID)
builder.Services.AddScoped<ILaborDurationStrategy, PassiveDurationStrategy>();
builder.Services.AddScoped<ILaborDurationStrategy, FixedTeamDurationStrategy>();
builder.Services.AddScoped<ILaborDurationStrategy, ScalableLaborDurationStrategy>();
builder.Services.AddScoped<StageDurationCalculator>();

builder.Services.AddSingleton<ShiftCalendarEngine>();
builder.Services.AddSingleton<MachineConflictDetector>();
builder.Services.AddSingleton<LaborDemandAggregator>();

builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IMachineTypeRepository, MachineTypeRepository>();
builder.Services.AddScoped<IOperatorSkillRepository, OperatorSkillRepository>();
builder.Services.AddScoped<IProductionOrderRepository, ProductionOrderRepository>();
builder.Services.AddScoped<IWorkShiftRepository, WorkShiftRepository>();

builder.Services.AddScoped<ProductionSchedulerService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Krono API v1");
    c.RoutePrefix = "swagger";
});

app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

// Database initialization and Seeding
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var context = services.GetRequiredService<KronoDbContext>();

    logger.LogInformation("Database provider in use: {Provider}", usePostgres ? "PostgreSQL" : "InMemory");
    await context.Database.EnsureCreatedAsync();
    await DbInitializer.SeedAsync(context);
    logger.LogInformation("Database seeded successfully with Cupcake pilot data.");
}

app.Run();
