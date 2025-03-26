using Application.Interfaces;
using Domain.Entities;
using Domain.Interfaces;
using Microsoft.Extensions.Logging;
using Infrastructure.Persistence.Repositories;

namespace Application.Services;

public class ArbitrageService : IArbitrageService
{
    private readonly IBinanceClient _binanceClient;
    private readonly IFuturesPriceRepository _futuresPriceRepository;
    private readonly ILogger<ArbitrageService> _logger;

    public ArbitrageService(IBinanceClient binanceClient, IFuturesPriceRepository futuresPriceRepository,
        ILogger<ArbitrageService> logger)
    {
        _binanceClient = binanceClient ?? throw new ArgumentNullException(nameof(binanceClient));
        _futuresPriceRepository = futuresPriceRepository ?? throw new ArgumentNullException(nameof(futuresPriceRepository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public async Task CalculateAndSaveArbitrage(DateTime startDate, DateTime endDate)
    {
        var coin1 = "BTCUSDT_QUARTER";
        var coin2 = "BTCUSDT_BI-QUARTER";

        decimal? lastPrice1 = null;
        decimal? lastPrice2 = null;

        // Попытка получить последнюю сохраненную цену из БД при старте.
        var lastSavedPrice = await _futuresPriceRepository.GetLastPriceAsync(coin1, coin2);
        if (lastSavedPrice != null)
        {
            // Используем последнюю цену, если она есть, для инициализации lastPrice1 и lastPrice2.
            _logger.LogInformation($"Using last known arbitrage difference {lastSavedPrice.PriceDifference} " +
                                   $"from {lastSavedPrice.Timestamp:yyyy-MM-dd} for symbols {coin1} and {coin2}.");
            // Запрос к API для получения цены coin1-2 на момент времени lastSavedPrice.Timestamp
            lastPrice1 = await _binanceClient.GetFuturesPriceAsync(coin1, lastSavedPrice.Timestamp); 
            lastPrice2 = await _binanceClient.GetFuturesPriceAsync(coin2, lastSavedPrice.Timestamp);
        }

        for (var date = startDate; date <= endDate; date = date.AddDays(1))
            try
            {
                var price1 = await _binanceClient.GetFuturesPriceAsync(coin1, date);
                if (price1 == null)
                {
                    price1 = lastPrice1;
                    _logger.LogWarning(
                        $"No data found for {coin1} on {date:yyyy-MM-dd}. Using last known price: {price1}");
                }

                var price2 = await _binanceClient.GetFuturesPriceAsync(coin2, date);
                if (price2 == null)
                {
                    price2 = lastPrice2;
                    _logger.LogWarning(
                        $"No data found for {coin2} on {date:yyyy-MM-dd}. Using last known price: {price2}");
                }

                if (price1.HasValue && price2.HasValue)
                {
                    var priceDifference = price1.Value - price2.Value;

                    var futuresPrice = new FuturesPrice
                    {
                        Id = Guid.NewGuid(),
                        Coin1 = coin1,
                        Coin2 = coin2,
                        Timestamp = date,
                        PriceDifference = priceDifference
                    };

                    await _futuresPriceRepository.AddAsync(futuresPrice);
                    _logger.LogInformation($"Arbitrage difference saved for {date:yyyy-MM-dd}: {priceDifference}");

                    lastPrice1 = price1;
                    lastPrice2 = price2;
                }
                else
                {
                    _logger.LogWarning($"Could not calculate arbitrage for {date:yyyy-MM-dd} due to missing data.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error calculating and saving arbitrage for {date:yyyy-MM-dd}.");
            }
    }
}

