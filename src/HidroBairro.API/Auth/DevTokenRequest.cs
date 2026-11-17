namespace HidroBairro.API.Auth;

public sealed class DevTokenRequest
{
    public long UsuarioId { get; init; }
    public string Papel { get; init; } = "CIDADAO";
}
