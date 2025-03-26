using Microsoft.Extensions.Configuration;
using Serilog;

namespace Infrastructure.Serilog;

public static class SerilogConfiguration
{
    public static void ConfigureSerilog(IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();
    }
}