using Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Persistence;

public class ArbitrageDbContext : DbContext
{
    public ArbitrageDbContext(DbContextOptions<ArbitrageDbContext> options) : base(options)
    {
    }

    public DbSet<FuturesPrice> FuturesPrices { get; set; }
}