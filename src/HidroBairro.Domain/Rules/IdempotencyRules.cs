using HidroBairro.Domain.Exceptions;

namespace HidroBairro.Domain.Rules;

public static class IdempotencyRules
{
    public const int MaxLength = 80;

    public static void EnsureKeyValida(string? idempotencyKey)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            throw new BusinessRuleException("Header Idempotency-Key é obrigatório.");

        var key = idempotencyKey.Trim();

        if (key.Length > MaxLength)
        {
            throw new BusinessRuleException(
                $"Idempotency-Key deve ter no máximo {MaxLength} caracteres.");
        }
    }

    public static string Normalize(string idempotencyKey) => idempotencyKey.Trim();
}
