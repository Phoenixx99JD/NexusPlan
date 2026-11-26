using System.Reflection;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using NexusPlan.Api.Exceptions;
using NexusPlan.Application;
using NexusPlan.Infrastructure;
using NexusPlan.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// ---------- Capas Clean Architecture ----------
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration, builder.Environment.IsEnvironment("Testing"));

// ---------- Manejo de excepciones ----------
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ---------- API + Swagger ----------
builder.Services
    .AddControllers()
    .AddJsonOptions(o => o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter()));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "NexusPlan API",
        Version = "v1",
        Description = "API REST para la gestión de proyectos y tareas (Clean Architecture - .NET 10)."
    });
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

builder.Services.AddHealthChecks().AddDbContextCheck<NexusPlanDbContext>("database");

var app = builder.Build();

// ---------- Inicialización / Migraciones automáticas ----------
if (app.Configuration.GetValue("Database:ApplyMigrationsOnStartup", true) && !app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<NexusPlanDbContext>();
    if (db.Database.IsSqlite())
        db.Database.EnsureCreated();
    else
        db.Database.Migrate();
}

app.UseExceptionHandler();
app.UseStatusCodePages();

// Swagger habilitado en todos los entornos para poder probar la API desplegada.
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "NexusPlan API v1");
    c.RoutePrefix = "swagger";
});

app.UseHttpsRedirection();
app.MapControllers();
app.MapHealthChecks("/health");
app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

/// <summary>Expuesto para pruebas de integración con WebApplicationFactory.</summary>
public partial class Program { }
