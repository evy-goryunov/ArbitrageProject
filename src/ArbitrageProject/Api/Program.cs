using Application.Interfaces;
using Application.Services;
using Domain.Interfaces;
using Infrastructure.Binance;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Serilog;
using Microsoft.EntityFrameworkCore;
using Serilog;
using Hangfire;
using Infrastructure.Hangfire;
using Hangfire.PostgreSql;

var builder = WebApplication.CreateBuilder(args);

SerilogConfiguration.ConfigureSerilog(builder.Configuration);
builder.Host.UseSerilog();

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<BinanceOptions>(builder.Configuration.GetSection("Binance"));

builder.Services.AddDbContext<ArbitrageDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IArbitrageService, ArbitrageService>();
builder.Services.AddScoped<IFuturesPriceRepository, FuturesPriceRepository>();

builder.Services.AddHttpClient<IBinanceClient, BinanceClient>();

builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UsePostgreSqlStorage(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddHangfireServer();
builder.Services.AddScoped<ArbitrageCalculationJob>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.UseHangfireDashboard();

app.MapControllers();

// Настройка для Hangfire job
RecurringJob.AddOrUpdate<ArbitrageCalculationJob>(
    "CalculateArbitrage",
    job => job.Execute(DateTime.Now.AddDays(-7), DateTime.Now),
    Cron.Daily(3, 0)); // Run at 3:00 AM

app.Run();