namespace HidroBairro.API.Auth;

public sealed class DevTokenRequest
{
    /// <summary>Email do usuário (mesmo formato do Java: claim sub).</summary>
    public string Email { get; init; } = string.Empty;

    public string Papel { get; init; } = "CIDADAO";
}
