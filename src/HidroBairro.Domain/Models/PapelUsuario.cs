namespace HidroBairro.Domain.Models;

/// <summary>Papéis de USUARIO_HB (domínio Java / claims JWT).</summary>
public static class PapelUsuario
{
    public const string Cidadao = "CIDADAO";
    public const string Gestor = "GESTOR";

    public static readonly IReadOnlySet<string> Todos = new HashSet<string>(StringComparer.Ordinal)
    {
        Cidadao,
        Gestor
    };

    public static bool IsValid(string? papel) =>
        !string.IsNullOrWhiteSpace(papel) && Todos.Contains(papel);
}
