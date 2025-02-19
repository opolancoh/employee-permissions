using Serilog;
using Microsoft.Extensions.Logging;

namespace EmployeePermissions.Api.Extensions;

public static class SerilogExtensions
{
    public static IHostBuilder AddLogging(this IHostBuilder builder)
    {
        builder
            .ConfigureLogging(logging => logging.ClearProviders())
            .UseSerilog((context, configuration) => 
                configuration.ReadFrom.Configuration(context.Configuration));

        return builder;
    }
}