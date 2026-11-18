using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderDiscount.Application.Configuration;
using OrderDiscount.Application.Interfaces;
using OrderDiscount.Domain.Entities;
using OrderDiscount.Domain.Exceptions;
using OrderDiscount.Domain.Interfaces;
namespace OrderDiscount.Application.Services;

public sealed class DiscountStrategyResolver(IEnumerable<IDiscountStrategy> strategies) : IDiscountStrategyResolver
{
    private readonly Dictionary<string, IDiscountStrategy> _strategies = strategies.ToDictionary(s => s.CustomerType, StringComparer.OrdinalIgnoreCase);

    public IDiscountStrategy Resolve(string customerType)
    {
        if (string.IsNullOrWhiteSpace(customerType) || !_strategies.TryGetValue(customerType.Trim(), out var strategy))
            throw new DomainValidationException("Error.UnsupportedCustomerType", $"Customer type '{customerType}' is not supported.", customerType);
        return strategy;
    }
    public IReadOnlyCollection<string> GetSupportedCustomerTypes() => _strategies.Keys.ToList().AsReadOnly();
}

public sealed class OrderDiscountService(
    IDiscountStrategyResolver resolver, IOrderRepository repo,
    IOptionsMonitor<DiscountSettings> settings, ILogger<OrderDiscountService> logger) : IOrderDiscountService
{
    public async Task<DiscountResult> CalculateAsync(Order order, CancellationToken ct = default)
    {
        ArgumentNullException.ThrowIfNull(order);
        var strategy = resolver.Resolve(order.CustomerType);
        var rounding = settings.CurrentValue.Rounding;

        var discountAmount = Math.Round(strategy.CalculateDiscount(order.OrderAmount), rounding.DecimalPlaces, rounding.Mode);
        var finalAmount = Math.Round(order.OrderAmount - discountAmount, rounding.DecimalPlaces, rounding.Mode);

        var result = new DiscountResult(order.OrderId, order.CustomerName, strategy.CustomerType, order.OrderAmount, strategy.GetDiscountRate(), discountAmount, finalAmount, DateTimeOffset.UtcNow);
        logger.LogInformation("Order {OrderId} ({CustomerType}): Original={Original}, Discount={Discount}, Final={Final}", result.OrderId, result.CustomerType, result.OriginalAmount, result.DiscountAmount, result.FinalAmount);

        await repo.SaveCalculationAsync(result, ct);
        return result;
    }
}