using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Models;
using HidroBairro.Domain.Rules;

namespace HidroBairro.UnitTests.Rules;

public class StatusTransitionRulesTests
{
    [Theory]
    [InlineData(SolicitacaoStatus.Recebida, SolicitacaoStatus.EmVistoria)]
    [InlineData(SolicitacaoStatus.Recebida, SolicitacaoStatus.Cancelada)]
    [InlineData(SolicitacaoStatus.EmVistoria, SolicitacaoStatus.Concluida)]
    [InlineData(SolicitacaoStatus.EmVistoria, SolicitacaoStatus.Cancelada)]
    public void EnsureTransicaoPermitida_CaminhosValidos_NaoLanca(string atual, string novo)
    {
        StatusTransitionRules.EnsureTransicaoPermitida(atual, novo);
    }

    [Theory]
    [InlineData(SolicitacaoStatus.Recebida, SolicitacaoStatus.Concluida)]
    [InlineData(SolicitacaoStatus.Concluida, SolicitacaoStatus.Recebida)]
    [InlineData(SolicitacaoStatus.Cancelada, SolicitacaoStatus.EmVistoria)]
    public void EnsureTransicaoPermitida_CaminhosInvalidos_Lanca(string atual, string novo)
    {
        Assert.Throws<BusinessRuleException>(() =>
            StatusTransitionRules.EnsureTransicaoPermitida(atual, novo));
    }

    [Fact]
    public void EnsurePapelPodeAlterar_Gestor_EmVistoria_NaoLanca()
    {
        StatusTransitionRules.EnsurePapelPodeAlterar(
            PapelUsuario.Gestor,
            SolicitacaoStatus.Recebida,
            SolicitacaoStatus.EmVistoria);
    }

    [Fact]
    public void EnsurePapelPodeAlterar_Cidadao_CancelarRecebida_NaoLanca()
    {
        StatusTransitionRules.EnsurePapelPodeAlterar(
            PapelUsuario.Cidadao,
            SolicitacaoStatus.Recebida,
            SolicitacaoStatus.Cancelada);
    }

    [Fact]
    public void EnsurePapelPodeAlterar_Cidadao_MoverParaVistoria_LancaForbidden()
    {
        Assert.Throws<ForbiddenException>(() =>
            StatusTransitionRules.EnsurePapelPodeAlterar(
                PapelUsuario.Cidadao,
                SolicitacaoStatus.Recebida,
                SolicitacaoStatus.EmVistoria));
    }
}
