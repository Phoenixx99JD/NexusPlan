using Microsoft.Extensions.DependencyInjection;
using NexusPlan.Application.Interfaces;
using NexusPlan.Application.Services;

namespace NexusPlan.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<ITaskService, TaskService>();
        return services;
    }
}
