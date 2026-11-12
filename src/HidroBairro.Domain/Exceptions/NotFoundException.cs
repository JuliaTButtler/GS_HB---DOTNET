namespace HidroBairro.Domain.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string message)
        : base("Não encontrado", message, 404)
    {
    }

    public static NotFoundException For(string resource, object id) =>
        new($"{resource} '{id}' não encontrado.");
}
