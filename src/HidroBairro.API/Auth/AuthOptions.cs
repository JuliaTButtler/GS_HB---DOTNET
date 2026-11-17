namespace HidroBairro.API.Auth;

public sealed class AuthOptions
{
    public const string SectionName = "Auth";

    /// <summary>Se false, endpoints não exigem JWT e usam headers X-User-Id / X-User-Role.</summary>
    public bool Enabled { get; set; }

    /// <summary>
    /// Emite JWT local em POST /api/auth/dev-token (Dev / fallback).
    /// Com Java oficial: pode deixar true só em Development.
    /// </summary>
    public bool LocalIssuerEnabled { get; set; }

    /// <summary>Secret compartilhado com o Java. Se KeyIsBase64=true, a string é Base64 do material HMAC.</summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>Java JwtService usa Base64.decode no secret.</summary>
    public bool KeyIsBase64 { get; set; } = true;

    /// <summary>Java atual não envia iss — manter false.</summary>
    public bool ValidateIssuer { get; set; }

    public string Issuer { get; set; } = string.Empty;

    /// <summary>Java atual não envia aud — manter false.</summary>
    public bool ValidateAudience { get; set; }

    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; } = 1440;
}
