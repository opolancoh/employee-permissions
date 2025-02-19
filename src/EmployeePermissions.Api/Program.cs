using EmployeePermissions.Api.Extensions;
using EmployeePermissions.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);

// This replaces the default logging provider with Serilog
builder.Host.AddLogging();

builder.Services.AddControllers();

// Add Infrastructure (DB, Kafka, ES)
builder.Services.AddInfrastructureServices(builder.Configuration);

var app = builder.Build();

app.MapControllers();

app.MigrateAndSeedDatabase();

app.Run();

// Make Program public so that tests can see it
public partial class Program
{
}