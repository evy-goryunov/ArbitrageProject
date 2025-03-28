using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;

namespace Tests.Infrastructure.Persistence.Repositories;

public class FuturesPriceRepositoryTests
{
    private ArbitrageDbContext _context;
    private FuturesPriceRepository _repository;

    [SetUp]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<ArbitrageDbContext>()
            .UseInMemoryDatabase(databaseName: "TestDatabase")
            .Options;

        _context = new ArbitrageDbContext(options);
        _context.Database.EnsureDeleted();
        _context.Database.EnsureCreated();

        _repository = new FuturesPriceRepository(_context);
    }
    
    [TearDown]
    public void TearDown()
    {
        _context.Dispose();
    }

    [Test]
    public async Task AddAsync_ValidFuturesPrice_AddsToDatabase()
    {
        // Arrange
        var futuresPrice = new FuturesPrice
        {
            Id = Guid.NewGuid(),
            Coin1 = "BTCUSDT_QUARTER",
            Coin2 = "BTCUSDT_BI-QUARTER",
            Timestamp = DateTime.Now.Date,
            PriceDifference = 1000
        };

        // Act
        await _repository.AddAsync(futuresPrice);

        // Assert
        var addedPrice = await _context.FuturesPrices.FindAsync(futuresPrice.Id);
        Assert.IsNotNull(addedPrice);
        Assert.AreEqual(futuresPrice.PriceDifference, addedPrice.PriceDifference);
    }

    [Test]
    public async Task GetLastPriceAsync_ExistingPrices_ReturnsLastPrice()
    {
        // Arrange
        var coin1 = "BTCUSDT_QUARTER";
        var coin2 = "BTCUSDT_BI-QUARTER";

        var futuresPrice1 = new FuturesPrice
        {
            Id = Guid.NewGuid(),
            Coin1 = coin1,
            Coin2 = coin2,
            Timestamp = DateTime.Now.Date.AddDays(-1),
            PriceDifference = 500
        };
        var futuresPrice2 = new FuturesPrice
        {
            Id = Guid.NewGuid(),
            Coin1 = coin1,
            Coin2 = coin2,
            Timestamp = DateTime.Now.Date,
            PriceDifference = 1000
        };

        await _context.FuturesPrices.AddRangeAsync(futuresPrice1, futuresPrice2);
        await _context.SaveChangesAsync();

        // Act
        var lastPrice = await _repository.GetLastPriceAsync(coin1, coin2);

        // Assert
        Assert.IsNotNull(lastPrice);
        Assert.AreEqual(futuresPrice2.PriceDifference, lastPrice.PriceDifference);
    }
}