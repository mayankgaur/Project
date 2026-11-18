namespace OrderDiscount.Domain.Interfaces;
public interface IDiscountStrategy
{
    string CustomerType { get; }
    decimal GetDiscountRate();
    decimal CalculateDiscount(decimal orderAmount);
}