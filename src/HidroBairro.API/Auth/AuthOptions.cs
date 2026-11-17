namespace HidroBairro.API.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Se false, endpoints não exigem JWT e usam headers X-User-Id / X-User-Role.</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Emite JWT local em POST /api/auth/dev-token (só para Dev / até o Java emitir).
    /// Desligar quando a API Java for o issuer oficial.
    /// </summary>
    public bool LocalIssuerEnabled { get; set; }

    public string Key { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 480;
}
