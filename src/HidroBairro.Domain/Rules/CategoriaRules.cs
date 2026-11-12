using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Models;

namespace HidroBairro.Domain.Rules;

public static class CategoriaRules
{
    public const int NomeMaxLength = 80;
    public const int DescricaoMaxLength = 255;

    public static void EnsureNomeValido(string? nome)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new BusinessRuleException("Nome da categoria é obrigatório.");

        if (nome.Trim().Length > NomeMaxLength)
            throw new BusinessRuleException($"Nome deve ter no máximo {NomeMaxLength} caracteres.");
    }

    public static void EnsureAtiva(string? ativo)
    {
        if (!AtivoFlags.IsAtivo(ativo))
            throw new BusinessRuleException("Categoria inativa não pode ser usada em novas solicitações.");
    }
}
