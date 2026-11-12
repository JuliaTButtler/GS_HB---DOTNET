namespace HidroBairro.Application.DTOs.Categorias;

public sealed class CategoriaDto
{
    public long Id { get; init; }
    public string Nome { get; init; } = string.Empty;
    public string? Descricao { get; init; }
    public string Ativo { get; init; } = "S";
}
