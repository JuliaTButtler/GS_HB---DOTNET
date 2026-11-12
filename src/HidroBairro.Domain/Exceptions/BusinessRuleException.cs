namespace HidroBairro.Domain.Exceptions;

public sealed class BusinessRuleException : AppException
{
    public BusinessRuleException(string message)
        : base("Regra de negócio", message, 400)
    {
    }

    private BusinessRuleException(string message, int statusCode)
        : base("Regra de negócio", message, statusCode)
    {
    }

    public static BusinessRuleException Conflict(string message) =>
        new(message, 409);
}
