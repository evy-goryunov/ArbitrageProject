using Application.Interfaces;
using Domain.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ArbitrageController : ControllerBase
{
    private readonly IArbitrageService _arbitrageService;
    private readonly ILogger<ArbitrageController> _logger;

    public ArbitrageController(IArbitrageService arbitrageService, ILogger<ArbitrageController> logger)
    {
        _arbitrageService = arbitrageService ?? throw new ArgumentNullException(nameof(arbitrageService));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [HttpPost("calculate")]
    public async Task<IActionResult> CalculateArbitrage(DateTime startDate, DateTime endDate)
    {
        try
        {
            await _arbitrageService.CalculateAndSaveArbitrage(startDate, endDate);
            return Ok("Arbitrage calculation started.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error starting arbitrage calculation.");
            return StatusCode(500, "Internal server error.");
        }
    }
}