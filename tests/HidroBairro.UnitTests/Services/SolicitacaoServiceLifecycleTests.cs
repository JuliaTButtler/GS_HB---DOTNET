using HidroBairro.Application.Abstractions;
using HidroBairro.Application.DTOs.Solicitacoes;
using HidroBairro.Application.Services;
using HidroBairro.Domain.Models;
using Moq;

namespace HidroBairro.UnitTests.Services;

public class SolicitacaoServiceLifecycleTests
{
    private readonly Mock<ISolicitacaoRepository> _solicitacoes = new();
    private readonly Mock<ICategoriaRepository> _categorias = new();
    private readonly Mock<IHistoricoStatusRepository> _historicos = new();
    private readonly Mock<IUnitOfWork> _uow = new();
    private readonly SolicitacaoService _sut;

    public SolicitacaoServiceLifecycleTests()
    {
        _sut = new SolicitacaoService(
            _solicitacoes.Object,
            _categorias.Object,
            _historicos.Object,
            _uow.Object);
    }

    [Fact]
    public async Task GetByIdAsync_Dono_RetornaSucesso()
    {
        var solicitacao = CriarSolicitacao(id: 5, usuarioId: 1);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        var result = await _sut.GetByIdAsync(5, usuarioId: 1, papel: PapelUsuario.Cidadao);

        Assert.True(result.Succeeded);
        Assert.Equal(5, result.Data!.Id);
    }

    [Fact]
    public async Task GetByIdAsync_OutroCidadao_RetornaForbidden()
    {
        var solicitacao = CriarSolicitacao(id: 5, usuarioId: 1);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(5, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        var result = await _sut.GetByIdAsync(5, usuarioId: 99, papel: PapelUsuario.Cidadao);

        Assert.False(result.Succeeded);
        Assert.True(result.Forbidden);
    }

    [Fact]
    public async Task GetByIdAsync_Inexistente_RetornaNotFound()
    {
        _solicitacoes
            .Setup(r => r.GetByIdAsync(404, It.IsAny<CancellationToken>()))
            .ReturnsAsync((SolicitacaoServico?)null);

        var result = await _sut.GetByIdAsync(404, usuarioId: 1, papel: PapelUsuario.Cidadao);

        Assert.False(result.Succeeded);
        Assert.True(result.NotFound);
    }

    [Fact]
    public async Task ListHistoricoAsync_Dono_RetornaItens()
    {
        var solicitacao = CriarSolicitacao(id: 7, usuarioId: 1);
        var historicos = new List<HistoricoStatus>
        {
            new()
            {
                Id = 1,
                SolicitacaoId = 7,
                StatusNovo = SolicitacaoStatus.Recebida,
                UsuarioId = 1,
                CriadoEm = DateTime.UtcNow
            }
        };

        _solicitacoes
            .Setup(r => r.GetByIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);
        _historicos
            .Setup(r => r.ListBySolicitacaoIdAsync(7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(historicos);

        var result = await _sut.ListHistoricoAsync(7, usuarioId: 1, papel: PapelUsuario.Cidadao);

        Assert.True(result.Succeeded);
        Assert.Single(result.Data!);
        Assert.Equal(SolicitacaoStatus.Recebida, result.Data![0].StatusNovo);
    }

    [Fact]
    public async Task UpdateAsync_DonoRecebida_AtualizaTitulo()
    {
        var solicitacao = CriarSolicitacao(id: 3, usuarioId: 1, status: SolicitacaoStatus.Recebida);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.UpdateAsync(
            3,
            new UpdateSolicitacaoRequest { Titulo = "Novo título", Descricao = "Nova descrição ok" },
            usuarioId: 1,
            papel: PapelUsuario.Cidadao);

        Assert.True(result.Succeeded);
        Assert.Equal("Novo título", result.Data!.Titulo);
        _solicitacoes.Verify(r => r.Update(solicitacao), Times.Once);
        _uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_StatusNaoRecebida_RetornaFailure()
    {
        var solicitacao = CriarSolicitacao(id: 3, usuarioId: 1, status: SolicitacaoStatus.EmVistoria);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        var result = await _sut.UpdateAsync(
            3,
            new UpdateSolicitacaoRequest { Titulo = "X", Descricao = "Y" },
            usuarioId: 1,
            papel: PapelUsuario.Cidadao);

        Assert.False(result.Succeeded);
        Assert.False(result.Forbidden);
        Assert.Contains("RECEBIDA", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UpdateAsync_OutroCidadao_RetornaForbidden()
    {
        var solicitacao = CriarSolicitacao(id: 3, usuarioId: 1);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(3, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        var result = await _sut.UpdateAsync(
            3,
            new UpdateSolicitacaoRequest { Titulo = "Hack", Descricao = "Hack" },
            usuarioId: 2,
            papel: PapelUsuario.Cidadao);

        Assert.True(result.Forbidden);
    }

    [Fact]
    public async Task CancelAsync_CidadaoDono_Cancela()
    {
        var solicitacao = CriarSolicitacao(id: 8, usuarioId: 1, status: SolicitacaoStatus.Recebida);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(8, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);
        _historicos
            .Setup(r => r.AddAsync(It.IsAny<HistoricoStatus>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.CancelAsync(8, usuarioId: 1, papel: PapelUsuario.Cidadao, observacao: "Desisti");

        Assert.True(result.Succeeded);
        Assert.Equal(SolicitacaoStatus.Cancelada, result.Data!.Status);
        _historicos.Verify(
            r => r.AddAsync(It.Is<HistoricoStatus>(h => h.StatusNovo == SolicitacaoStatus.Cancelada), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task ChangeStatusAsync_Gestor_RecebidaParaEmVistoria()
    {
        var solicitacao = CriarSolicitacao(id: 9, usuarioId: 1, status: SolicitacaoStatus.Recebida);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);
        _historicos
            .Setup(r => r.AddAsync(It.IsAny<HistoricoStatus>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        _uow.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var result = await _sut.ChangeStatusAsync(
            9,
            new ChangeStatusRequest { StatusNovo = SolicitacaoStatus.EmVistoria, Observacao = "Equipe a caminho" },
            usuarioId: 12,
            papel: PapelUsuario.Gestor);

        Assert.True(result.Succeeded);
        Assert.Equal(SolicitacaoStatus.EmVistoria, result.Data!.Status);
    }

    [Fact]
    public async Task ChangeStatusAsync_CidadaoTentaEmVistoria_RetornaForbidden()
    {
        var solicitacao = CriarSolicitacao(id: 9, usuarioId: 1, status: SolicitacaoStatus.Recebida);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        var result = await _sut.ChangeStatusAsync(
            9,
            new ChangeStatusRequest { StatusNovo = SolicitacaoStatus.EmVistoria },
            usuarioId: 1,
            papel: PapelUsuario.Cidadao);

        Assert.True(result.Forbidden);
        Assert.Contains("cancelar", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangeStatusAsync_TransicaoInvalida_RetornaFailure()
    {
        var solicitacao = CriarSolicitacao(id: 9, usuarioId: 1, status: SolicitacaoStatus.Recebida);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        var result = await _sut.ChangeStatusAsync(
            9,
            new ChangeStatusRequest { StatusNovo = SolicitacaoStatus.Concluida },
            usuarioId: 12,
            papel: PapelUsuario.Gestor);

        Assert.False(result.Succeeded);
        Assert.False(result.Forbidden);
        Assert.Contains("não é permitida", result.Error!, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangeStatusAsync_CidadaoNaoDono_RetornaForbidden()
    {
        var solicitacao = CriarSolicitacao(id: 9, usuarioId: 1, status: SolicitacaoStatus.Recebida);
        _solicitacoes
            .Setup(r => r.GetByIdAsync(9, It.IsAny<CancellationToken>()))
            .ReturnsAsync(solicitacao);

        var result = await _sut.ChangeStatusAsync(
            9,
            new ChangeStatusRequest { StatusNovo = SolicitacaoStatus.Cancelada },
            usuarioId: 2,
            papel: PapelUsuario.Cidadao);

        Assert.True(result.Forbidden);
    }

    private static SolicitacaoServico CriarSolicitacao(
        long id,
        long usuarioId,
        string status = SolicitacaoStatus.Recebida) =>
        new()
        {
            Id = id,
            Protocolo = $"HB-TEST-{id}",
            UsuarioId = usuarioId,
            CategoriaId = 1,
            Titulo = "Bueiro entupido",
            Descricao = "Teste unitário",
            Status = status,
            Latitude = -23.65m,
            Longitude = -46.75m,
            CriadoEm = DateTime.UtcNow,
            AtualizadoEm = DateTime.UtcNow
        };
}
