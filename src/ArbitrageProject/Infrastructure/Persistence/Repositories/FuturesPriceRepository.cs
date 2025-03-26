using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence.Repositories;

public class FuturesPriceRepository : IFuturesPriceRepository
{
    private readonly ArbitrageDbContext _context;

    public FuturesPriceRepository(ArbitrageDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    public async Task AddAsync(FuturesPrice futuresPrice)
    {
        _context.FuturesPrices.Add(futuresPrice);
        await _context.SaveChangesAsync();
    }

    public async Task<FuturesPrice?> GetLastPriceAsync(string symbol1, string symbol2)
    {
        return await _context.FuturesPrices
            .Where(fp => fp.Coin1 == symbol1 && fp.Coin2 == symbol2)
            .OrderByDescending(fp => fp.Timestamp)
            .FirstOrDefaultAsync();
    }
}