using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Application.Interfaces;
using EmployeePermissions.Application.Services;
using EmployeePermissions.Core.Interfaces;
using EmployeePermissions.Infrastructure.Elasticsearch;
using EmployeePermissions.Infrastructure.Kafka;
using EmployeePermissions.Infrastructure.Persistence;
using EmployeePermissions.Infrastructure.Repositories;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace EmployeePermissions.Infrastructure.Extensions;

public static class InfrastructureExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration config)
    {
        // DB Context
        var connectionString = config.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString));

        // Repositories
        services.AddScoped<IEmployeeRepository, EmployeeRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IPermissionTypeRepository, PermissionTypeRepository>();

        // Unit Of Work
        services.AddScoped<IRepositoryManager, RepositoryManager>();

        // Services
        services.AddScoped<IPermissionService, PermissionService>();

        // Kafka
        services.AddOptions<KafkaOptions>()
            .BindConfiguration(nameof(KafkaOptions));
        services.AddSingleton<IKafkaService, KafkaService>();

        // Elasticsearch
        services.AddOptions<ElasticsearchOptions>()
            .BindConfiguration(nameof(ElasticsearchOptions));
        services.AddSingleton<IElasticsearchService, ElasticsearchService>();

        return services;
    }


    public static IApplicationBuilder MigrateAndSeedDatabase(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Ensure the database is created & migrations are applied
        dbContext.Database.Migrate();

        // DataSeeder.SeedInitialData(dbContext);

        return app;
    }
}