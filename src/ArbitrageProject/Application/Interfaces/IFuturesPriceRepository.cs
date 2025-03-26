using Domain.Entities;

namespace Application.Interfaces;

public interface IFuturesPriceRepository
{
    Task AddAsync(FuturesPrice futuresPrice);
    Task<FuturesPrice?> GetLastPriceAsync(string symbol1, string symbol2);
}