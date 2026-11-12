namespace HidroBairro.Domain.Models;

/// <summary>
/// Referência somente-leitura a PONTO_MONITORAMENTO_HB (dono: Java).
/// Usada quando a solicitação liga a um ponto monitorado.
/// </summary>
public class PontoMonitoramentoRef
{
    public long Id { get; set; }

    public string Nome { get; set; } = string.Empty;

    public string Subprefeitura { get; set; } = string.Empty;

    public string Bairro { get; set; } = string.Empty;

    public string Zona { get; set; } = "SUL";

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string Ativo { get; set; } = AtivoFlags.Sim;
}
