using Domain.Entities;

namespace Infrastructure.Persistence.Repositories;

public interface IFuturesPriceRepository
{
    Task AddAsync(FuturesPrice futuresPrice);
    Task<FuturesPrice?> GetLastPriceAsync(string symbol1, string symbol2);
}