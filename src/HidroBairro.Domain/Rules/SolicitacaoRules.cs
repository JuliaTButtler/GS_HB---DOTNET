using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Models;

namespace HidroBairro.Domain.Rules;

public static class SolicitacaoRules
{
    public const int TituloMaxLength = 120;
    public const int DescricaoMaxLength = 1000;
    public const int ProtocoloMaxLength = 30;
    public const int IdempotencyKeyMaxLength = 80;

    /// <summary>
    /// CHK_SOLICITACAO_HB_LOCAL: PONTO_ID NOT NULL OU (LATITUDE e LONGITUDE NOT NULL).
    /// </summary>
    public static void EnsureLocalInformado(long? pontoId, decimal? latitude, decimal? longitude)
    {
        if (pontoId is > 0)
            return;

        if (latitude is null || longitude is null)
        {
            throw new BusinessRuleException(
                "Informe um ponto de monitoramento ou latitude e longitude.");
        }
    }

    public static void EnsureTituloValido(string? titulo)
    {
        if (string.IsNullOrWhiteSpace(titulo))
            throw new BusinessRuleException("Título é obrigatório.");

        if (titulo.Trim().Length > TituloMaxLength)
            throw new BusinessRuleException($"Título deve ter no máximo {TituloMaxLength} caracteres.");
    }

    public static void EnsureDescricaoValida(string? descricao)
    {
        if (string.IsNullOrWhiteSpace(descricao))
            throw new BusinessRuleException("Descrição é obrigatória.");

        if (descricao.Trim().Length > DescricaoMaxLength)
            throw new BusinessRuleException($"Descrição deve ter no máximo {DescricaoMaxLength} caracteres.");
    }

    public static void EnsureCategoriaInformada(long categoriaId)
    {
        if (categoriaId <= 0)
            throw new BusinessRuleException("Categoria é obrigatória.");
    }

    public static void EnsureUsuarioInformado(long usuarioId)
    {
        if (usuarioId <= 0)
            throw new BusinessRuleException("Usuário é obrigatório.");
    }

    /// <summary>Cidadão só edita/cancela enquanto status = RECEBIDA.</summary>
    public static void EnsureEditavelPorCidadao(string status)
    {
        if (!string.Equals(status, SolicitacaoStatus.Recebida, StringComparison.Ordinal))
        {
            throw new BusinessRuleException(
                "Só é possível alterar ou cancelar solicitação com status RECEBIDA.");
        }
    }
}
