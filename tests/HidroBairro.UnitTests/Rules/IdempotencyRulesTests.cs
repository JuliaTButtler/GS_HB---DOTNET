using HidroBairro.Domain.Exceptions;
using HidroBairro.Domain.Rules;

namespace HidroBairro.UnitTests.Rules;

public class IdempotencyRulesTests
{
    [Fact]
    public void EnsureKeyValida_KeyOk_NaoLanca()
    {
        IdempotencyRules.EnsureKeyValida("mobile-abc-123");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void EnsureKeyValida_Vazia_Lanca(string? key)
    {
        Assert.Throws<BusinessRuleException>(() => IdempotencyRules.EnsureKeyValida(key));
    }

    [Fact]
    public void EnsureKeyValida_MaiorQue80_Lanca()
    {
        var key = new string('x', IdempotencyRules.MaxLength + 1);

        Assert.Throws<BusinessRuleException>(() => IdempotencyRules.EnsureKeyValida(key));
    }

    [Fact]
    public void Normalize_RemoveEspacos()
    {
        Assert.Equal("abc", IdempotencyRules.Normalize("  abc  "));
    }
}
