namespace HidroBairro.Domain.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Não autorizado.")
        : base("Não autorizado", message, 401)
    {
    }
}
