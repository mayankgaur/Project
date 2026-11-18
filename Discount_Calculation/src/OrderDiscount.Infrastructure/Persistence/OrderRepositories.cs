using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderDiscount.Application.Configuration;
using OrderDiscount.Application.Interfaces;
using OrderDiscount.Domain.Entities;

namespace OrderDiscount.Infrastructure.Persistence;

public sealed class NoOpOrderRepository(ILogger<NoOpOrderRepository> logger) : IOrderRepository
{
    public Task SaveCalculationAsync(DiscountResult result, CancellationToken ct = default)
    {
        logger.LogDebug("Database disabled. Skipping persistence for Order {OrderId}.", result.OrderId);
        return Task.CompletedTask;
    }
}

public sealed class SqlOrderRepository(IOptionsMonitor<DatabaseSettings> dbSettings, ILogger<SqlOrderRepository> logger) : IOrderRepository
{
    public async Task SaveCalculationAsync(DiscountResult result, CancellationToken ct = default)
    {
        var connStr = dbSettings.CurrentValue.ConnectionString;
        if (string.IsNullOrWhiteSpace(connStr))
            throw new InvalidOperationException("DatabaseSettings:ConnectionString is required when Enabled is true.");

        const string sql = """
            INSERT INTO dbo.OrderDiscountCalculations (OrderId, CustomerName, CustomerType, OriginalAmount, DiscountRate, DiscountAmount, FinalAmount, CalculatedAtUtc)
            VALUES (@OrderId, @CustomerName, @CustomerType, @OriginalAmount, @DiscountRate, @DiscountAmount, @FinalAmount, @CalculatedAtUtc);
            """;

        await using var conn = new SqlConnection(connStr);
        await conn.OpenAsync(ct);
        await using var cmd = new SqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@OrderId", result.OrderId);
        cmd.Parameters.AddWithValue("@CustomerName", result.CustomerName);
        cmd.Parameters.AddWithValue("@CustomerType", result.CustomerType);
        cmd.Parameters.AddWithValue("@OriginalAmount", result.OriginalAmount);
        cmd.Parameters.AddWithValue("@DiscountRate", result.DiscountRate);
        cmd.Parameters.AddWithValue("@DiscountAmount", result.DiscountAmount);
        cmd.Parameters.AddWithValue("@FinalAmount", result.FinalAmount);
        cmd.Parameters.AddWithValue("@CalculatedAtUtc", result.CalculatedAtUtc);
        await cmd.ExecuteNonQueryAsync(ct);
        logger.LogInformation("Saved Order {OrderId} to SQL database.", result.OrderId);
    }
}

public static class PersistenceExtensions
{
    public static IServiceCollection AddConfigurablePersistence(this IServiceCollection services, IConfiguration config)
    {
        var db = config.GetSection(DatabaseSettings.SectionName).Get<DatabaseSettings>() ?? new DatabaseSettings();
        if (db.Enabled) services.AddScoped<IOrderRepository, SqlOrderRepository>();
        else services.AddScoped<IOrderRepository, NoOpOrderRepository>();
        return services;
    }
}