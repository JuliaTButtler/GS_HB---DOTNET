using HidroBairro.Application.Abstractions;
using HidroBairro.Application.DTOs.Solicitacoes;
using HidroBairro.Application.Services;
using HidroBairro.Domain.Models;
using Moq;

namespace HidroBairro.UnitTests.Services;

public class SolicitacaoServiceTests
{
    private readonly Mock<ISolicitacaoRepository> _solicitacoes = new();
    private readonly Mock<ICategoriaRepository> _categorias = new();
    private readonly Mock<IHistoricoStatusRepository> _historicos = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly SolicitacaoService _sut;

    public SolicitacaoServiceTests()
    {
        _sut = new SolicitacaoService(
            _solicitacoes.Object,
            _categorias.Object,
            _historicos.Object,
            _uow.Object);
    }

    [Fact]
    public async Task CreateAsync_NovaSolicitacao_RetornaSucesso()
    {
        var categoria = new CategoriaSolicitacao
        {
            Id = 2,
            Nome = "Boca de lobo",
            Ativo = AtivoFlags.Sim
        };

        _solicitacoes
            .Setup(r => r.GetByIdempotencyKeyAsync("key-1", It.IsAny<CancellationToken>()))
            .ReturnsAsync((SolicitacaoServico?)null);

        _categorias
            .Setup(r => r.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categoria);

        _solicitacoes
            .Setup(r => r.AddAsync(It.IsAny<SolicitacaoServico>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _historicos
            .Setup(r => r.AddAsync(It.IsAny<HistoricoStatus>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var request = new CreateSolicitacaoRequest
        {
            CategoriaId = 2,
            PontoId = 14,
            Titulo = "Boca de lobo residencial",
            Descricao = "Obstrucao em rua estreita"
        };

        var result = await _sut.CreateAsync(request, usuarioId: 3, idempotencyKey: "key-1");

        Assert.True(result.Succeeded);
        Assert.True(result.Created);
        Assert.NotNull(result.Data);
        Assert.Equal(SolicitacaoStatus.Recebida, result.Data!.Status);
        Assert.Equal(2, result.Data.CategoriaId);
        Assert.Equal("key-1", result.Data.IdempotencyKey);
        Assert.StartsWith("HB-", result.Data.Protocolo);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_MesmaIdempotencyKey_RetornaExistenteSemSalvarDeNovo()
    {
        var existente = new SolicitacaoServico
        {
            Id = 10,
            Protocolo = "HB-20261112-ABCDEF12",
            UsuarioId = 3,
            CategoriaId = 2,
            Titulo = "Já criada",
            Descricao = "Desc",
            Status = SolicitacaoStatus.Recebida,
            IdempotencyKey = "key-dup",
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };

        _solicitacoes
            .Setup(r => r.GetByIdempotencyKeyAsync("key-dup", It.IsAny<CancellationToken>()))
            .ReturnsAsync(existente);

        var request = new CreateSolicitacaoRequest
        {
            CategoriaId = 2,
            Latitude = -23.6m,
            Longitude = -46.7m,
            Titulo = "Ignorado",
            Descricao = "Ignorado"
        };

        var result = await _sut.CreateAsync(request, usuarioId: 3, idempotencyKey: "key-dup");

        Assert.True(result.Succeeded);
        Assert.False(result.Created);
        Assert.Equal(10, result.Data!.Id);
        Assert.Equal("HB-20261112-ABCDEF12", result.Data.Protocolo);
        _solicitacoes.Verify(
            r => r.AddAsync(It.IsAny<SolicitacaoServico>(), It.IsAny<CancellationToken>()),
            Times.Never);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ListAsync_Cidadao_FiltraPeloUsuario()
    {
        var items = new List<SolicitacaoServico>
        {
            new()
            {
                Id = 1,
                Protocolo = "HB-1",
                UsuarioId = 3,
                CategoriaId = 1,
                Titulo = "A",
                Descricao = "B",
                Status = SolicitacaoStatus.Recebida,
                CriadoEm = DateTime.UtcNow,
                AtualizadoEm = DateTime.UtcNow
            }
        };

        _solicitacoes
            .Setup(r => r.ListAsync(3, null, 1, 10, It.IsAny<CancellationToken>()))
            .ReturnsAsync((items, 1));

        var result = await _sut.ListAsync(
            new SolicitacaoListQuery { Page = 1, PageSize = 10 },
            usuarioId: 3,
            papel: PapelUsuario.Cidadao);

        Assert.True(result.Succeeded);
        Assert.Single(result.Data!.Items);
        Assert.Equal(1, result.Data.Total);
        _solicitacoes.Verify(
            r => r.ListAsync(3, null, 1, 10, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_SemLocal_RetornaFailure()
    {
        _solicitacoes
            .Setup(r => r.GetByIdempotencyKeyAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((SolicitacaoServico?)null);

        var request = new CreateSolicitacaoRequest
        {
            CategoriaId = 1,
            Titulo = "Sem local",
            Descricao = "Falta ponto e lat/long"
        };

        var result = await _sut.CreateAsync(request, usuarioId: 1, idempotencyKey: "key-x");

        Assert.False(result.Succeeded);
        Assert.Contains("ponto", result.Error!, StringComparison.OrdinalIgnoreCase);
    }
}
