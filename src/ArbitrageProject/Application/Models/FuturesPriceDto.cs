namespace Application.Models;

public class FuturesPriceDto
{
    public Guid Id { get; set; }
    public string Coin1 { get; set; }
    public string Coin2 { get; set; }
    public DateTime Timestamp { get; set; }
    public decimal PriceDifference { get; set; }
}