namespace Application.Interfaces;

public interface IBinanceClient
{
    Task<decimal?> GetFuturesPriceAsync(string coin, DateTime timestamp);
}