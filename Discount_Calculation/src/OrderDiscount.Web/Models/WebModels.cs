using OrderDiscount.Domain.Entities;
namespace OrderDiscount.Web.Models;

public sealed record CalculateDiscountRequest(string OrderId, string CustomerName, decimal OrderAmount, string CustomerType);

public sealed record CalculateDiscountResponse(
    string OrderId, string CustomerName, string CustomerType,
    decimal OriginalAmount, decimal DiscountPercentage, decimal DiscountAmount,
    decimal FinalAmount, DateTimeOffset CalculatedAtUtc)
{
    public static CalculateDiscountResponse FromDomain(DiscountResult r) =>
        new(r.OrderId, r.CustomerName, r.CustomerType, r.OriginalAmount, r.DiscountRate * 100m, r.DiscountAmount, r.FinalAmount, r.CalculatedAtUtc);
}

public sealed class OrderFormViewModel
{
    public string OrderId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public decimal? OrderAmount { get; set; }
    public string CustomerType { get; set; } = "Regular";
    public string Culture { get; set; } = "en-US";
    public CalculateDiscountResponse? Result { get; set; }
    public string? ErrorMessage { get; set; }
    public IReadOnlyCollection<string> AvailableCustomerTypes { get; set; } = Array.Empty<string>();
}