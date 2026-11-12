using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Models;
using HidroBairro.Domain.Rules;

namespace HidroBairro.UnitTests.Rules;

public class SolicitacaoRulesTests
{
    [Fact]
    public void EnsureLocalInformado_ComPontoId_NaoLanca()
    {
        SolicitacaoRules.EnsureLocalInformado(pontoId: 1, latitude: null, longitude: null);
    }

    [Fact]
    public void EnsureLocalInformado_ComLatLong_NaoLanca()
    {
        SolicitacaoRules.EnsureLocalInformado(pontoId: null, latitude: -23.6m, longitude: -46.7m);
    }

    [Fact]
    public void EnsureLocalInformado_SemPontoNemCoordenadas_LancaBusinessRule()
    {
        var ex = Assert.Throws<BusinessRuleException>(() =>
            SolicitacaoRules.EnsureLocalInformado(null, null, null));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public void EnsureEditavelPorCidadao_StatusRecebida_NaoLanca()
    {
        SolicitacaoRules.EnsureEditavelPorCidadao(SolicitacaoStatus.Recebida);
    }

    [Theory]
    [InlineData(SolicitacaoStatus.EmVistoria)]
    [InlineData(SolicitacaoStatus.Concluida)]
    [InlineData(SolicitacaoStatus.Cancelada)]
    public void EnsureEditavelPorCidadao_OutrosStatus_Lanca(string status)
    {
        Assert.Throws<BusinessRuleException>(() =>
            SolicitacaoRules.EnsureEditavelPorCidadao(status));
    }
}
