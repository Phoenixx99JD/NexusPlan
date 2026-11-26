using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Time.Testing;
using NexusPlan.Domain.Repositories;
using NexusPlan.Infrastructure.Data;
using NexusPlan.Infrastructure.Repositories;

namespace NexusPlan.Tests.Unit;

/// <summary>Base de datos SQLite en memoria (relacional) y repositorios para pruebas unitarias de casos de uso.</summary>
public sealed class TestDb : IDisposable
{
    private readonly SqliteConnection _connection;
    public NexusPlanDbContext Context { get; }
    public IProjectRepository Projects { get; }
    public ITaskRepository Tasks { get; }
    public FakeTimeProvider Clock { get; } = new(new DateTimeOffset(2026, 6, 15, 12, 0, 0, TimeSpan.Zero));

    public TestDb()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();
        var options = new DbContextOptionsBuilder<NexusPlanDbContext>().UseSqlite(_connection).Options;
        Context = new NexusPlanDbContext(options);
        Context.Database.EnsureCreated();
        Projects = new ProjectRepository(Context);
        Tasks = new TaskRepository(Context);
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
