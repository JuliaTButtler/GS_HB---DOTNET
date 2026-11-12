using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Models;
using HidroBairro.Domain.Rules;

namespace HidroBairro.UnitTests.Rules;

public class CategoriaRulesTests
{
    [Fact]
    public void EnsureAtiva_ComS_NaoLanca()
    {
        CategoriaRules.EnsureAtiva(AtivoFlags.Sim);
    }

    [Fact]
    public void EnsureAtiva_ComN_Lanca()
    {
        Assert.Throws<BusinessRuleException>(() => CategoriaRules.EnsureAtiva(AtivoFlags.Nao));
    }

    [Fact]
    public void EnsureNomeValido_Vazio_Lanca()
    {
        Assert.Throws<BusinessRuleException>(() => CategoriaRules.EnsureNomeValido(" "));
    }
}
