using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using OrderDiscount.Application.Configuration;
using OrderDiscount.Application.Services;
using OrderDiscount.Application.Strategies;
using OrderDiscount.Domain.Entities;
using OrderDiscount.Domain.Exceptions;
using OrderDiscount.Domain.Interfaces;
using OrderDiscount.Infrastructure.Persistence;
using Xunit;

namespace OrderDiscount.Tests;

public sealed class OrderDiscountTests
{
    private static OrderDiscountService CreateService(DiscountSettings? custom = null)
    {
        var settings = custom ?? new DiscountSettings
        {
            Rates = new(StringComparer.OrdinalIgnoreCase) { ["Regular"] = 0.05m, ["Premium"] = 0.10m, ["VIP"] = 0.15m },
            Rounding = new RoundingSettings { DecimalPlaces = 2, Mode = MidpointRounding.AwayFromZero }
        };
        var monitor = new StaticMonitor<DiscountSettings>(settings);
        var resolver = new DiscountStrategyResolver(new IDiscountStrategy[] { new RegularDiscountStrategy(monitor), new PremiumDiscountStrategy(monitor), new VipDiscountStrategy(monitor) });
        return new OrderDiscountService(resolver, new NoOpOrderRepository(NullLogger<NoOpOrderRepository>.Instance), monitor, NullLogger<OrderDiscountService>.Instance);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-10)]
    public void Order_ZeroOrNegativeAmount_Throws(decimal amount)
    {
        var ex = Assert.Throws<DomainValidationException>(() => new Order("ORD-1", "Alice", amount, "Regular"));
        Assert.Equal("Error.OrderAmountInvalid", ex.ErrorCode);
    }

    [Theory]
    [InlineData("Regular", 200.00, 10.00, 190.00)]
    [InlineData("Premium", 200.00, 20.00, 180.00)]
    [InlineData("VIP", 200.00, 30.00, 170.00)]
    public async Task CalculateAsync_ValidTiers_ReturnsExpectedAmounts(string tier, decimal amount, decimal expectedDisc, decimal expectedFinal)
    {
        var result = await CreateService().CalculateAsync(new Order("ORD-100", "John", amount, tier));
        Assert.Equal(expectedDisc, result.DiscountAmount);
        Assert.Equal(expectedFinal, result.FinalAmount);
    }

    [Fact]
    public async Task CalculateAsync_UnsupportedTier_Throws()
    {
        var ex = await Assert.ThrowsAsync<DomainValidationException>(() => CreateService().CalculateAsync(new Order("ORD-2", "Bob", 100m, "Gold")));
        Assert.Equal("Error.UnsupportedCustomerType", ex.ErrorCode);
    }

    private sealed class StaticMonitor<T>(T val) : IOptionsMonitor<T>
    {
        public T CurrentValue { get; } = val;
        public T Get(string? name) => CurrentValue;
        public IDisposable? OnChange(Action<T, string?> listener) => null;
    }
}