namespace OrderDiscount.Application.Configuration;
public sealed class DiscountSettings
{
    public const string SectionName = "DiscountSettings";
    public Dictionary<string, decimal> Rates { get; set; } = new(StringComparer.OrdinalIgnoreCase);
    public RoundingSettings Rounding { get; set; } = new();
}
public sealed class RoundingSettings
{
    public int DecimalPlaces { get; set; } = 2;
    public MidpointRounding Mode { get; set; } = MidpointRounding.AwayFromZero;
}