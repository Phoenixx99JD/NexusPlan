using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NexusPlan.Domain.Repositories;
using NexusPlan.Infrastructure.Data;
using NexusPlan.Infrastructure.Repositories;

namespace NexusPlan.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        bool isTestingEnvironment)
    {
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<ITaskRepository, TaskRepository>();

        if (!isTestingEnvironment)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            var provider = configuration.GetValue("Database:Provider", "SqlServer");

            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Falta la cadena de conexión 'ConnectionStrings:DefaultConnection'.");

            services.AddDbContext<NexusPlanDbContext>(options =>
            {
                if (string.Equals(provider, "Sqlite", StringComparison.OrdinalIgnoreCase)
                    || connectionString.StartsWith("Data Source=", StringComparison.OrdinalIgnoreCase))
                {
                    options.UseSqlite(connectionString);
                }
                else
                {
                    options.UseSqlServer(connectionString, sql =>
                        sql.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null));
                }
            });
        }

        return services;
    }
}
