using Application.Interfaces;
using Application.Services;
using Domain.Entities;
using Microsoft.Extensions.Logging;
using Moq;

namespace Tests.Application.Services;

public class ArbitrageServiceTests
{
    private Mock<IBinanceClient> _binanceClientMock;
    private Mock<IFuturesPriceRepository> _futuresPriceRepositoryMock;
    private Mock<ILogger<ArbitrageService>> _loggerMock;
    private ArbitrageService _arbitrageService;

    [SetUp]
    public void Setup()
    {
        _binanceClientMock = new Mock<IBinanceClient>();
        _futuresPriceRepositoryMock = new Mock<IFuturesPriceRepository>();
        _loggerMock = new Mock<ILogger<ArbitrageService>>();

        _arbitrageService = new ArbitrageService(
            _binanceClientMock.Object,
            _futuresPriceRepositoryMock.Object,
            _loggerMock.Object);
    }

    [Test]
    public async Task CalculateAndSaveArbitrage_ValidData_SavesPriceDifference()
    {
        // Arrange
        var startDate = DateTime.Now.Date;
        var endDate = DateTime.Now.Date;
        var coin1 = "BTCUSDT_QUARTER";
        var coin2 = "BTCUSDT_BI-QUARTER";
        decimal price1 = 20000;
        decimal price2 = 19000;

        _binanceClientMock.Setup(x => x.GetFuturesPriceAsync(coin1, startDate))
            .ReturnsAsync(price1);
        _binanceClientMock.Setup(x => x.GetFuturesPriceAsync(coin2, startDate))
            .ReturnsAsync(price2);

        // Act
        await _arbitrageService.CalculateAndSaveArbitrage(startDate, endDate);

        // Assert
        _futuresPriceRepositoryMock.Verify(
            x => x.AddAsync(It.Is<FuturesPrice>(fp =>
                fp.Coin1 == coin1 &&
                fp.Coin2 == coin2 &&
                fp.PriceDifference == price1 - price2 &&
                fp.Timestamp == startDate)),
            Times.Once);

        _loggerMock.Verify(
            x => x.Log(
                LogLevel.Information,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((o, t) =>
                    o.ToString()!.Contains(
                        $"Arbitrage difference saved for {startDate:yyyy-MM-dd}: {price1 - price2}")),
                It.IsAny<Exception>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            Times.Once);
    }
}