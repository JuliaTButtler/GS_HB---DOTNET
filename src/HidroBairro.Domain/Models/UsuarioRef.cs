namespace HidroBairro.Domain.Models;

/// <summary>
/// Referência somente-leitura a USUARIO_HB (dono: Java).
/// Usada para FKs; .NET não faz CRUD nesta tabela.
/// </summary>
public class UsuarioRef
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public string Papel { get; set; } = string.Empty;

    public string Ativo { get; set; } = AtivoFlags.Sim;
}
