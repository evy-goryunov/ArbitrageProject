namespace Domain.Interfaces;

public interface IArbitrageService
{
    Task CalculateAndSaveArbitrage(DateTime startDate, DateTime endDate);
}