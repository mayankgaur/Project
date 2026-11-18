namespace OrderDiscount.Domain.Exceptions;
public sealed class DomainValidationException(string errorCode, string defaultMessage, params object[] args) : Exception(defaultMessage)
{
    public string ErrorCode { get; } = errorCode;
    public object[] Args { get; } = args;
}