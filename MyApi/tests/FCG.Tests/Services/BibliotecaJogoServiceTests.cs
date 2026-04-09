using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using FCG.Domain.Entities;
using Moq;
using System.Linq.Expressions;

namespace FCG.Tests.Services
{
    public class BibliotecaJogoServiceTests
    {
        private readonly Mock<IBibliotecaJogoRepository> _bibliotecaRepositoryMock;
        private readonly Mock<IJogoRepository> _jogoRepositoryMock;
        private readonly Mock<IPromocaoRepository> _promocaoRepositoryMock;
        private readonly BibliotecaJogoService _service;

        public BibliotecaJogoServiceTests()
        {
            _bibliotecaRepositoryMock = new Mock<IBibliotecaJogoRepository>();
            _jogoRepositoryMock = new Mock<IJogoRepository>();
            _promocaoRepositoryMock = new Mock<IPromocaoRepository>();
            _service = new BibliotecaJogoService(
                _bibliotecaRepositoryMock.Object,
                _jogoRepositoryMock.Object,
                _promocaoRepositoryMock.Object);
        }

        #region GetBibliotecaByUsuarioIdAsync

        [Fact]
        public async Task GetBibliotecaByUsuarioIdAsync_RetornaItens()
        {
            var userId = Guid.NewGuid();
            var itens = new List<BibliotecaJogo>
            {
                new() { Id = Guid.NewGuid(), UsuarioId = userId, JogoId = Guid.NewGuid(), PrecoPago = 50m, DataCompra = DateTime.UtcNow, Jogo = new Jogo { Nome = "Jogo1" } },
                new() { Id = Guid.NewGuid(), UsuarioId = userId, JogoId = Guid.NewGuid(), PrecoPago = 30m, DataCompra = DateTime.UtcNow, Jogo = new Jogo { Nome = "Jogo2" } }
            };
            _bibliotecaRepositoryMock.Setup(r => r.GetByUsuarioIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(itens);

            var result = await _service.GetBibliotecaByUsuarioIdAsync(userId);

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.Count());
        }

        #endregion

        #region GetByIdAsync

        [Fact]
        public async Task GetByIdAsync_Existe_ReturnoSucesso()
        {
            var item = new BibliotecaJogo
            {
                Id = Guid.NewGuid(),
                UsuarioId = Guid.NewGuid(),
                JogoId = Guid.NewGuid(),
                PrecoPago = 50m,
                DataCompra = DateTime.UtcNow,
                Jogo = new Jogo { Nome = "Jogo" }
            };
            _bibliotecaRepositoryMock.Setup(r => r.GetByIdAsync(item.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(item);

            var result = await _service.GetByIdAsync(item.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(item.Id, result.Data!.Id);
        }

        [Fact]
        public async Task GetByIdAsync_NaoEncontrado_ReturnoFalha_404()
        {
            _bibliotecaRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((BibliotecaJogo?)null);

            var result = await _service.GetByIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region AddJogoToBibliotecaAsync

        [Fact]
        public async Task AddJogoToBibliotecaAsync_ValidarComPromocao_RetornoSucesso_201()
        {
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var jogo = new Jogo { Id = jogoId, Nome = "Jogo", Preco = 100m, Ativo = true };

            _bibliotecaRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BibliotecaJogo, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BibliotecaJogo>());
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jogo);

            var dto = new CreateBibliotecaJogoDTO { JogoId = jogoId };

            var result = await _service.AddJogoToBibliotecaAsync(userId, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal(100m, result.Data!.PrecoPago);
        }

        [Fact]
        public async Task AddJogoToBibliotecaAsync_ComPercentualPromocao_ApplicarDesconto()
        {
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocaoId = Guid.NewGuid();
            var jogo = new Jogo { Id = jogoId, Nome = "Jogo", Preco = 100m, Ativo = true };
            var promocao = new Promocao
            {
                Id = promocaoId,
                Ativa = true,
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 20m,
                DataInicio = DateTime.UtcNow.AddDays(-1),
                DataFim = DateTime.UtcNow.AddDays(10)
            };

            _bibliotecaRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BibliotecaJogo, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BibliotecaJogo>());
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jogo);
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(promocaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);

            var dto = new CreateBibliotecaJogoDTO { JogoId = jogoId, PromocaoId = promocaoId };

            var result = await _service.AddJogoToBibliotecaAsync(userId, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(80m, result.Data!.PrecoPago);
        }

        [Fact]
        public async Task AddJogoToBibliotecaAsync_ComValorFixoPromocao_ApplicarDesconto()
        {
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocaoId = Guid.NewGuid();
            var jogo = new Jogo { Id = jogoId, Nome = "Jogo", Preco = 100m, Ativo = true };
            var promocao = new Promocao
            {
                Id = promocaoId,
                Ativa = true,
                TipoDesconto = TipoDesconto.ValorFixo,
                Valor = 30m,
                DataInicio = DateTime.UtcNow.AddDays(-1),
                DataFim = DateTime.UtcNow.AddDays(10)
            };

            _bibliotecaRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BibliotecaJogo, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BibliotecaJogo>());
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jogo);
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(promocaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);

            var dto = new CreateBibliotecaJogoDTO { JogoId = jogoId, PromocaoId = promocaoId };

            var result = await _service.AddJogoToBibliotecaAsync(userId, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(70m, result.Data!.PrecoPago);
        }

        [Fact]
        public async Task AddJogoToBibliotecaAsync_DiscontoSuperiorPreco_PrecoZERO()
        {
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocaoId = Guid.NewGuid();
            var jogo = new Jogo { Id = jogoId, Nome = "Jogo", Preco = 10m, Ativo = true };
            var promocao = new Promocao
            {
                Id = promocaoId,
                Ativa = true,
                TipoDesconto = TipoDesconto.ValorFixo,
                Valor = 50m,
                DataInicio = DateTime.UtcNow.AddDays(-1),
                DataFim = DateTime.UtcNow.AddDays(10)
            };

            _bibliotecaRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BibliotecaJogo, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BibliotecaJogo>());
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jogo);
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(promocaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);

            var dto = new CreateBibliotecaJogoDTO { JogoId = jogoId, PromocaoId = promocaoId };

            var result = await _service.AddJogoToBibliotecaAsync(userId, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(0m, result.Data!.PrecoPago);
        }

        [Fact]
        public async Task AddJogoToBibliotecaAsync_JogoExisteNaBiblioteca_RetornaFalha_409()
        {
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var existing = new BibliotecaJogo { Id = Guid.NewGuid(), UsuarioId = userId, JogoId = jogoId };

            _bibliotecaRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BibliotecaJogo, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<BibliotecaJogo> { existing });

            var dto = new CreateBibliotecaJogoDTO { JogoId = jogoId };

            var result = await _service.AddJogoToBibliotecaAsync(userId, dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(409, result.StatusCode);
        }

        [Fact]
        public async Task AddJogoToBibliotecaAsync_JogoNaoEncontrado_RetornaFalha_404()
        {
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();

            _bibliotecaRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BibliotecaJogo, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BibliotecaJogo>());
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Jogo?)null);

            var dto = new CreateBibliotecaJogoDTO { JogoId = jogoId };

            var result = await _service.AddJogoToBibliotecaAsync(userId, dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task AddJogoToBibliotecaAsync_JogoInativo_RetornoaFalha_400()
        {
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var jogo = new Jogo { Id = jogoId, Nome = "Jogo", Preco = 100m, Ativo = false };

            _bibliotecaRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BibliotecaJogo, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BibliotecaJogo>());
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jogo);

            var dto = new CreateBibliotecaJogoDTO { JogoId = jogoId };

            var result = await _service.AddJogoToBibliotecaAsync(userId, dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task AddJogoToBibliotecaAsync_ExpiredoPromocao_RetornaFalha_400()
        {
            var userId = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocaoId = Guid.NewGuid();
            var jogo = new Jogo { Id = jogoId, Nome = "Jogo", Preco = 100m, Ativo = true };
            var promocao = new Promocao
            {
                Id = promocaoId,
                Ativa = true,
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 10m,
                DataInicio = DateTime.UtcNow.AddDays(-30),
                DataFim = DateTime.UtcNow.AddDays(-1)
            };

            _bibliotecaRepositoryMock.Setup(r => r.FindAsync(It.IsAny<Expression<Func<BibliotecaJogo, bool>>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Enumerable.Empty<BibliotecaJogo>());
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jogo);
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(promocaoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);

            var dto = new CreateBibliotecaJogoDTO { JogoId = jogoId, PromocaoId = promocaoId };

            var result = await _service.AddJogoToBibliotecaAsync(userId, dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        #endregion

        #region RemoveJogoFromBibliotecaAsync

        [Fact]
        public async Task RemoveJogoFromBibliotecaAsync_Existe_ReturnaSucesso()
        {
            var id = Guid.NewGuid();
            _bibliotecaRepositoryMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.RemoveJogoFromBibliotecaAsync(id);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task RemoveJogoFromBibliotecaAsync_NaoEncontrado_RetornaFalha_404()
        {
            _bibliotecaRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _service.RemoveJogoFromBibliotecaAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion
    }
}
