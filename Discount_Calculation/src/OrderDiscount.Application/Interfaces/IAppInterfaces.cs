using OrderDiscount.Domain.Entities;
using OrderDiscount.Domain.Interfaces;
namespace OrderDiscount.Application.Interfaces;

public interface IDiscountStrategyResolver
{
    IDiscountStrategy Resolve(string customerType);
    IReadOnlyCollection<string> GetSupportedCustomerTypes();
}
public interface IOrderDiscountService
{
    Task<DiscountResult> CalculateAsync(Order order, CancellationToken cancellationToken = default);
}
public interface IOrderRepository
{
    Task SaveCalculationAsync(DiscountResult result, CancellationToken cancellationToken = default);
}