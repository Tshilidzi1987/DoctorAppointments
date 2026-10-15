using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore.InMemory;
using Application.Interfaces.Repositories;
using Infrastructure.Persistence;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(configuration);

        bool useInMemory = bool.TryParse(configuration["UseInMemoryDatabase"], out var parsed) && parsed;

        if (useInMemory)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase("AppDb_InMemory"));
        }
        else
        {
            string? connectionString = configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException("Connection string 'DefaultConnection' was not found.");
            }

            services.AddDbContext<AppDbContext>(options => options.UseSqlServer(connectionString));
        }

        // Register repositories
        services.AddScoped<IAttendeesRepository, AttendeesRepository>();
        services.AddScoped<IEventsRepository, EventsRepository>();
        services.AddScoped<IEventAttendanceRepository, EventAttendanceRepository>();

        return services;
    }
}