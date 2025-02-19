using EmployeePermissions.Application.DTOs;
using EmployeePermissions.Application.Interfaces;
using EmployeePermissions.Core.Entities;
using EmployeePermissions.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;

namespace EmployeePermissions.IntegrationTests.Common;

public abstract class BaseIntegrationTest : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly WebApplicationFactory<Program> Factory;
    protected readonly string DatabaseName;

    protected BaseIntegrationTest()
    {
        DatabaseName = $"PermissionsDb_Test_{Guid.NewGuid():N}";

        // Load configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .AddJsonFile("appsettings.Test.json", optional: true) 
            .AddUserSecrets<Program>()
            .Build();

        var baseConnectionString = configuration.GetConnectionString("DefaultConnection")
                                   ?? throw new InvalidOperationException(
                                       "Database connection string not found in appsettings.json");

        Factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Test");
                
                builder.ConfigureServices(services =>
                {
                    // Remove existing DbContextOptions
                    var descriptor = services
                        .SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    var testConnectionString = $"{baseConnectionString};Database={DatabaseName}";
                    services.AddDbContext<ApplicationDbContext>(options => options.UseSqlServer(testConnectionString));
                });
            });

        Client = Factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        // Ensure the database is created & migrations are applied
        await dbContext.Database.MigrateAsync();
    }


    public async Task DisposeAsync()
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.Database.EnsureDeletedAsync();
    }

    protected async Task<Employee> CreateEmployeeAsync(string name)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var employee = new Employee { Id = Guid.NewGuid(), Name = name };

        await dbContext.Employees.AddAsync(employee);
        await dbContext.SaveChangesAsync();

        return employee;
    }

    protected async Task<PermissionType> CreatePermissionTypeAsync(string name, string? description = null)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var permissionType = new PermissionType { Id = Guid.NewGuid(), Name = name, Description = description };

        await dbContext.PermissionTypes.AddAsync(permissionType);
        await dbContext.SaveChangesAsync();

        return permissionType;
    }

    protected async Task<Permission> CreatePermissionAsync(
        Guid employeeId,
        Guid permissionTypeId,
        DateTime grantedDate,
        string? description = null)
    {
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var permission = new Permission
        {
            EmployeeId = employeeId,
            PermissionTypeId = permissionTypeId,
            GrantedDate = grantedDate,
            Description = description
        };

        await dbContext.Permissions.AddAsync(permission);
        await dbContext.SaveChangesAsync();

        return permission;
    }

    protected async Task CreateAndIndexElasticsearchPermissionAsync(Permission permission)
    {
        using var scope = Factory.Services.CreateScope();
        var elasticsearchService = scope.ServiceProvider.GetRequiredService<IElasticsearchService>();

        await elasticsearchService.IndexPermissionAsync(permission);
    }
}