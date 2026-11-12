using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Models;

namespace HidroBairro.Domain.Rules;

public static class StatusTransitionRules
{
    private static readonly Dictionary<string, HashSet<string>> TransicoesPermitidas =
        new(StringComparer.Ordinal)
        {
            [SolicitacaoStatus.Recebida] =
            [
                SolicitacaoStatus.EmVistoria,
                SolicitacaoStatus.Cancelada
            ],
            [SolicitacaoStatus.EmVistoria] =
            [
                SolicitacaoStatus.Concluida,
                SolicitacaoStatus.Cancelada
            ],
            [SolicitacaoStatus.Concluida] = [],
            [SolicitacaoStatus.Cancelada] = []
        };

    public static void EnsureStatusValido(string? status)
    {
        if (!SolicitacaoStatus.IsValid(status))
        {
            throw new BusinessRuleException(
                $"Status inválido. Use: {string.Join(", ", SolicitacaoStatus.Todos)}.");
        }
    }

    public static void EnsureTransicaoPermitida(string statusAtual, string statusNovo)
    {
        EnsureStatusValido(statusAtual);
        EnsureStatusValido(statusNovo);

        if (string.Equals(statusAtual, statusNovo, StringComparison.Ordinal))
            throw new BusinessRuleException("O novo status deve ser diferente do atual.");

        if (!TransicoesPermitidas.TryGetValue(statusAtual, out var destinos) ||
            !destinos.Contains(statusNovo))
        {
            throw new BusinessRuleException(
                $"Transição de '{statusAtual}' para '{statusNovo}' não é permitida.");
        }
    }

    /// <summary>
    /// CIDADAO: cancela a própria (RECEBIDA → CANCELADA).
    /// GESTOR: alterações operacionais (vistoria/conclusão/cancelamento).
    /// </summary>
    public static void EnsurePapelPodeAlterar(string papel, string statusAtual, string statusNovo)
    {
        if (!PapelUsuario.IsValid(papel))
            throw new ForbiddenException("Papel de usuário inválido.");

        EnsureTransicaoPermitida(statusAtual, statusNovo);

        if (string.Equals(papel, PapelUsuario.Gestor, StringComparison.Ordinal))
            return;

        // Cidadão: apenas cancelar enquanto RECEBIDA
        var cancelamentoCidadao =
            string.Equals(statusAtual, SolicitacaoStatus.Recebida, StringComparison.Ordinal) &&
            string.Equals(statusNovo, SolicitacaoStatus.Cancelada, StringComparison.Ordinal);

        if (!cancelamentoCidadao)
        {
            throw new ForbiddenException(
                "Cidadão só pode cancelar solicitação com status RECEBIDA.");
        }
    }
}
