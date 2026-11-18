using OrderDiscount.Domain.Exceptions;
namespace OrderDiscount.Domain.Entities;

public sealed class Order
{
    public string OrderId { get; }
    public string CustomerName { get; }
    public decimal OrderAmount { get; }
    public string CustomerType { get; }

    public Order(string orderId, string customerName, decimal orderAmount, string customerType)
    {
        if (string.IsNullOrWhiteSpace(orderId)) throw new DomainValidationException("Error.OrderIdRequired", "Order ID is required.");
        if (string.IsNullOrWhiteSpace(customerName)) throw new DomainValidationException("Error.CustomerNameRequired", "Customer name is required.");
        if (orderAmount <= 0) throw new DomainValidationException("Error.OrderAmountInvalid", "Order amount must be greater than zero.");
        if (string.IsNullOrWhiteSpace(customerType)) throw new DomainValidationException("Error.CustomerTypeRequired", "Customer type is required.");

        OrderId = orderId.Trim();
        CustomerName = customerName.Trim();
        OrderAmount = orderAmount;
        CustomerType = customerType.Trim();
    }
}