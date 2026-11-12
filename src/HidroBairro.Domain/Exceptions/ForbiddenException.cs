namespace HidroBairro.Domain.Exceptions;

public sealed class ForbiddenException : AppException
{
    public ForbiddenException(string message = "Acesso negado.")
        : base("Acesso negado", message, 403)
    {
    }
}
