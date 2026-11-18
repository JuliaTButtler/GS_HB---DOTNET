using HidroBairro.Domain.Models;
using HidroBairro.Domain.Rules;

namespace HidroBairro.UnitTests;

/// <summary>
/// Smoke mínimo do domínio — evita Assert.True(true) sem valor.
/// </summary>
public class SmokeTests
{
    [Fact]
    public void SolicitacaoStatus_Validos_IncluemFluxoOperacional()
    {
        Assert.True(SolicitacaoStatus.IsValid(SolicitacaoStatus.Recebida));
        Assert.True(SolicitacaoStatus.IsValid(SolicitacaoStatus.EmVistoria));
        Assert.True(SolicitacaoStatus.IsValid(SolicitacaoStatus.Concluida));
        Assert.True(SolicitacaoStatus.IsValid(SolicitacaoStatus.Cancelada));
        Assert.False(SolicitacaoStatus.IsValid("INVALIDO"));
    }

    [Fact]
    public void IdempotencyRules_Normalize_Trim()
    {
        Assert.Equal("abc-123", IdempotencyRules.Normalize("  abc-123  "));
    }
}
