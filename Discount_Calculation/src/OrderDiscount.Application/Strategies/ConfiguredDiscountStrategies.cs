using Microsoft.Extensions.Options;
using OrderDiscount.Application.Configuration;
using OrderDiscount.Domain.Exceptions;
using OrderDiscount.Domain.Interfaces;
namespace OrderDiscount.Application.Strategies;

public abstract class ConfiguredRateStrategyBase(IOptionsMonitor<DiscountSettings> settings) : IDiscountStrategy
{
    public abstract string CustomerType { get; }
    public decimal GetDiscountRate()
    {
        if (!settings.CurrentValue.Rates.TryGetValue(CustomerType, out var rate) || rate < 0 || rate > 1)
            throw new DomainValidationException("Error.RateNotConfigured", $"Discount rate for '{CustomerType}' is missing or invalid.", CustomerType);
        return rate;
    }
    public decimal CalculateDiscount(decimal orderAmount) => orderAmount * GetDiscountRate();
}

public sealed class RegularDiscountStrategy(IOptionsMonitor<DiscountSettings> s) : ConfiguredRateStrategyBase(s) { public override string CustomerType => "Regular"; }
public sealed class PremiumDiscountStrategy(IOptionsMonitor<DiscountSettings> s) : ConfiguredRateStrategyBase(s) { public override string CustomerType => "Premium"; }
public sealed class VipDiscountStrategy(IOptionsMonitor<DiscountSettings> s) : ConfiguredRateStrategyBase(s) { public override string CustomerType => "VIP"; }