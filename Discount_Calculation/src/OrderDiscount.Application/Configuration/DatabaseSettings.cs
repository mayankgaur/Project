namespace OrderDiscount.Application.Configuration;
public sealed class DatabaseSettings
{
    public const string SectionName = "DatabaseSettings";
    public bool Enabled { get; set; } = false;
    public string Provider { get; set; } = "SqlServer";
    public string ConnectionString { get; set; } = string.Empty;
}