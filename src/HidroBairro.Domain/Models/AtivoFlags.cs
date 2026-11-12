namespace HidroBairro.Domain.Models;

/// <summary>Oracle CHAR(1) usado em ATIVO ('S'/'N').</summary>
public static class AtivoFlags
{
    public const string Sim = "S";
    public const string Nao = "N";

    public static bool IsAtivo(string? valor) =>
        string.Equals(valor, Sim, StringComparison.OrdinalIgnoreCase);
}
