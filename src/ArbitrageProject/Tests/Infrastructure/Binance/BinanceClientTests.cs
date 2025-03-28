using System.Net;
using Infrastructure.Binance;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;

namespace Tests.Infrastructure.Binance;

public class BinanceClientTests
{
    private Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private HttpClient _httpClient;
    private Mock<IOptions<BinanceOptions>> _optionsMock;
    private Mock<ILogger<BinanceClient>> _loggerMock;
    private BinanceClient _binanceClient;

    [SetUp]
    public void Setup()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();
        _httpClient = new HttpClient(_httpMessageHandlerMock.Object);
        _optionsMock = new Mock<IOptions<BinanceOptions>>();
        _loggerMock = new Mock<ILogger<BinanceClient>>();

        _optionsMock.Setup(x => x.Value).Returns(new BinanceOptions {BaseUrl = "https://test.binance.com"});

        _binanceClient = new BinanceClient(_httpClient, _optionsMock.Object, _loggerMock.Object);
    }

    [TearDown]
    public void TearDown()
    {
        _httpClient?.Dispose();
        _httpMessageHandlerMock?.VerifyAll();
    }

    //Этот тест сейчас не пройдёт. Написал как пример.
    [Test]
    public async Task GetFuturesPriceAsync_ValidResponse_ReturnsPrice()
    {
        // Arrange
        var coin = "BTCUSDT_QUARTER";
        var timestamp = DateTime.Now.Date;
        decimal expectedPrice = 20000;
        var jsonResponse =
            $"[[{new DateTimeOffset(timestamp).ToUnixTimeMilliseconds()},\"19900\",\"20100\",\"19800\",\"" +
            expectedPrice +
            "\",\"100\",\"1678886459999\",\"2352000.00\",\"1000\",\"50.00\",\"1176000.00\",\"0\"]]"; // Mocked Klines data

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var price = await _binanceClient.GetFuturesPriceAsync(coin, timestamp);

        // Assert
        Assert.IsNotNull(price);
        Assert.AreEqual(expectedPrice, price);
    }

    [Test]
    public async Task GetFuturesPriceAsync_NoData_ReturnsNull()
    {
        // Arrange
        var coin = "BTCUSDT_QUARTER";
        var timestamp = DateTime.Now.Date;
        var jsonResponse = "[]";

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>()
            )
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent(jsonResponse)
            });

        // Act
        var price = await _binanceClient.GetFuturesPriceAsync(coin, timestamp);

        // Assert
        Assert.IsNull(price);
    }
}