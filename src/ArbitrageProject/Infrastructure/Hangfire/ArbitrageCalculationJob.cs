using Domain.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Infrastructure.Hangfire;

public class ArbitrageCalculationJob
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ArbitrageCalculationJob> _logger;

    public ArbitrageCalculationJob(IServiceProvider serviceProvider, ILogger<ArbitrageCalculationJob> logger)
    {
        _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task Execute(DateTime startDate, DateTime endDate)
    {
        _logger.LogInformation($"Arbitrage calculation job started for period: {startDate:yyyy-MM-dd} - {endDate:yyyy-MM-dd}");

        using (var scope = _serviceProvider.CreateScope())
        {
            var arbitrageService = scope.ServiceProvider.GetRequiredService<IArbitrageService>();

            try
            {
                await arbitrageService.CalculateAndSaveArbitrage(startDate, endDate);
                _logger.LogInformation("Arbitrage calculation job completed successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred during arbitrage calculation job.");
            }
        }
    }
}