using System.Net.Http.Json;
using Application.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Infrastructure.Binance;

public class BinanceClient : IBinanceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<BinanceClient> _logger;
    private readonly BinanceOptions _options;

    public BinanceClient(HttpClient httpClient, IOptions<BinanceOptions> options, ILogger<BinanceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));

        _httpClient.BaseAddress = new Uri(_options.BaseUrl);
    }

    public async Task<decimal?> GetFuturesPriceAsync(string coin, DateTime timestamp)
    {
        try
        {
            var apiUrl = $"/fapi/v1/klines?symbol={coin}&interval=1d&limit=1&startTime={new DateTimeOffset(timestamp).ToUnixTimeMilliseconds()}";

            var response = await _httpClient.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<List<object>>>();
                if (result != null && result.Count > 0)
                {
                    // предполагаю что индекс цены закрытия в Klines data = 4
                    var price = Convert.ToDecimal(result[0][4]);
                    return price;
                }

                _logger.LogWarning($"No data found for {coin} at {timestamp}.");
                return null;
            }

            _logger.LogError($"Error fetching data from Binance API for {coin} at {timestamp}: {response.StatusCode}");
            return null;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, $"HTTP request error while fetching data for {coin} at {timestamp}.");
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error fetching or processing data for {coin} at {timestamp}: {ex.Message}");
            return null;
        }
    }
}