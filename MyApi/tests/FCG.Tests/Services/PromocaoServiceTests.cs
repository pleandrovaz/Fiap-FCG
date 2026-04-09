using Application.DTOs;
using Application.Services;
using Domain.Enums;
using Domain.Interfaces.Repositories;
using FCG.Domain.Entities;
using Moq;

namespace FCG.Tests.Services
{
    public class PromocaoServiceTests
    {
        private readonly Mock<IPromocaoRepository> _promocaoRepositoryMock;
        private readonly Mock<IJogoRepository> _jogoRepositoryMock;
        private readonly PromocaoService _service;

        public PromocaoServiceTests()
        {
            _promocaoRepositoryMock = new Mock<IPromocaoRepository>();
            _jogoRepositoryMock = new Mock<IJogoRepository>();
            _service = new PromocaoService(_promocaoRepositoryMock.Object, _jogoRepositoryMock.Object);
        }

        #region GetAllPromocoesAsync

        [Fact]
        public async Task GetAllPromocoesAsync_ReturnsAll()
        {
            var promocoes = new List<Promocao>
            {
                new() { Id = Guid.NewGuid(), Nome = "Promo1", IdJogo = Guid.NewGuid(), Valor = 10m, TipoDesconto = TipoDesconto.Percentual, DataInicio = DateTime.UtcNow, DataFim = DateTime.UtcNow.AddDays(10), Ativa = true },
                new() { Id = Guid.NewGuid(), Nome = "Promo2", IdJogo = Guid.NewGuid(), Valor = 5m, TipoDesconto = TipoDesconto.ValorFixo, DataInicio = DateTime.UtcNow, DataFim = DateTime.UtcNow.AddDays(5), Ativa = false }
            };
            _promocaoRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocoes);

            var result = await _service.GetAllPromocoesAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.Count());
        }

        #endregion

        #region GetPromocaoByIdAsync

        [Fact]
        public async Task GetPromocaoByIdAsync_ExisteRetornoSucessos()
        {
            var promocao = new Promocao
            {
                Id = Guid.NewGuid(),
                Nome = "Promo",
                IdJogo = Guid.NewGuid(),
                Valor = 10m,
                TipoDesconto = TipoDesconto.Percentual,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10),
                Ativa = true
            };
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(promocao.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);

            var result = await _service.GetPromocaoByIdAsync(promocao.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(promocao.Nome, result.Data!.Nome);
        }

        [Fact]
        public async Task GetPromocaoByIdAsync_NaoEncontrado_RetornaFalha_404()
        {
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Promocao?)null);

            var result = await _service.GetPromocaoByIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region CreatePromocaoAsync

        [Fact]
        public async Task CreatePromocaoAsync_ValidaData_ReturnaSucesso_201()
        {
            var jogoId = Guid.NewGuid();
            var dto = new CreatePromocaoDTO
            {
                Nome = "Nova Promo",
                IdJogo = jogoId,
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 15m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(30)
            };
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Jogo { Id = jogoId, Nome = "Jogo" });

            var result = await _service.CreatePromocaoAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            _promocaoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Promocao>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task CreatePromocaoAsync_DataFimMenorQueDataInicio_ReturnaFalha_400()
        {
            var dto = new CreatePromocaoDTO
            {
                Nome = "Promo",
                IdJogo = Guid.NewGuid(),
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 10m,
                DataInicio = DateTime.UtcNow.AddDays(10),
                DataFim = DateTime.UtcNow
            };

            var result = await _service.CreatePromocaoAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreatePromocaoAsync_JogoNaoEncontrado_RetornaFalha_404()
        {
            var dto = new CreatePromocaoDTO
            {
                Nome = "Promo",
                IdJogo = Guid.NewGuid(),
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 10m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10)
            };
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(dto.IdJogo, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Jogo?)null);

            var result = await _service.CreatePromocaoAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region UpdatePromocaoAsync

        [Fact]
        public async Task UpdatePromocaoAsyncDataValidaReturnSucesso200()
        {
            var id = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocao = new Promocao
            {
                Id = id,
                Nome = "Old",
                IdJogo = jogoId,
                Valor = 10m,
                TipoDesconto = TipoDesconto.Percentual,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10),
                Ativa = true
            };
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Jogo { Id = jogoId, Nome = "Jogo" });

            var dto = new UpdatePromocaoDTO
            {
                Nome = "Updated",
                IdJogo = jogoId,
                TipoDesconto = TipoDesconto.ValorFixo,
                Valor = 20m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(20)
            };

            var result = await _service.UpdatePromocaoAsync(id, dto);

            Assert.True(result.IsSuccess);
            _promocaoRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Promocao>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdatePromocaoAsync_NaoEncontrado_RetornaFalha_404()
        {
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Promocao?)null);

            var dto = new UpdatePromocaoDTO
            {
                Nome = "Test",
                IdJogo = Guid.NewGuid(),
                Valor = 10m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10)
            };

            var result = await _service.UpdatePromocaoAsync(Guid.NewGuid(), dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        [Fact]
        public async Task UpdatePromocaoAsync_DataFimAnteriorDataInicio_ReturnFalha_400()
        {
            var id = Guid.NewGuid();
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Promocao { Id = id, Nome = "P", IdJogo = Guid.NewGuid() });

            var dto = new UpdatePromocaoDTO
            {
                Nome = "Test",
                IdJogo = Guid.NewGuid(),
                Valor = 10m,
                DataInicio = DateTime.UtcNow.AddDays(10),
                DataFim = DateTime.UtcNow
            };

            var result = await _service.UpdatePromocaoAsync(id, dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task UpdatePromocaoAsync_JogoNaoEncontrado_RetornaFalha_404()
        {
            var id = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Promocao { Id = id, Nome = "P", IdJogo = jogoId });
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Jogo?)null);

            var dto = new UpdatePromocaoDTO
            {
                Nome = "Test",
                IdJogo = jogoId,
                Valor = 10m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10)
            };

            var result = await _service.UpdatePromocaoAsync(id, dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region DeletePromocaoAsync

        [Fact]
        public async Task DeletePromocaoAsync_ExisteRetornoSucessos()
        {
            var id = Guid.NewGuid();
            _promocaoRepositoryMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.DeletePromocaoAsync(id);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task DeletePromocaoAsync_NaoEncontrado_RetornaFalha_404()
        {
            _promocaoRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _service.DeletePromocaoAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region Edge Cases - Create

        [Fact]
        public async Task CreatePromocaoAsync_DataFimIgualDataInicio_RetornaFalha_400()
        {
            var agora = DateTime.UtcNow;
            var dto = new CreatePromocaoDTO
            {
                Nome = "Promo",
                IdJogo = Guid.NewGuid(),
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 10m,
                DataInicio = agora,
                DataFim = agora
            };

            var result = await _service.CreatePromocaoAsync(dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        [Fact]
        public async Task CreatePromocaoAsync_MapeiaTodosCamposCorretamente()
        {
            var jogoId = Guid.NewGuid();
            var dto = new CreatePromocaoDTO
            {
                Nome = "Promo Mapeamento",
                IdJogo = jogoId,
                TipoDesconto = TipoDesconto.ValorFixo,
                Valor = 25.50m,
                DataInicio = new DateTime(2025, 1, 1),
                DataFim = new DateTime(2025, 12, 31)
            };
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Jogo { Id = jogoId, Nome = "Jogo Teste" });

            var result = await _service.CreatePromocaoAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            var data = result.Data!;
            Assert.Equal("Promo Mapeamento", data.Nome);
            Assert.Equal(jogoId, data.IdJogo);
            Assert.Equal("Jogo Teste", data.JogoNome);
            Assert.Equal(TipoDesconto.ValorFixo, data.TipoDesconto);
            Assert.Equal(25.50m, data.Valor);
            Assert.Equal(new DateTime(2025, 1, 1), data.DataInicio);
            Assert.Equal(new DateTime(2025, 12, 31), data.DataFim);
            Assert.True(data.Ativa);
        }

        #endregion

        #region Edge Cases - Update

        [Fact]
        public async Task UpdatePromocaoAsync_ValorZero_MantemValorOriginal()
        {
            var id = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocao = new Promocao
            {
                Id = id,
                Nome = "Promo",
                IdJogo = jogoId,
                Valor = 50m,
                TipoDesconto = TipoDesconto.Percentual,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10),
                Ativa = true
            };
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Jogo { Id = jogoId, Nome = "Jogo" });

            var dto = new UpdatePromocaoDTO
            {
                Nome = "Promo",
                IdJogo = jogoId,
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 0m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10)
            };

            var result = await _service.UpdatePromocaoAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(50m, promocao.Valor);
        }

        [Fact]
        public async Task UpdatePromocaoAsync_TipoDescontoZero_MantemOriginal()
        {
            var id = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocao = new Promocao
            {
                Id = id,
                Nome = "Promo",
                IdJogo = jogoId,
                Valor = 10m,
                TipoDesconto = TipoDesconto.ValorFixo,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10),
                Ativa = true
            };
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Jogo { Id = jogoId, Nome = "Jogo" });

            var dto = new UpdatePromocaoDTO
            {
                Nome = "Promo",
                IdJogo = jogoId,
                TipoDesconto = 0,
                Valor = 10m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10)
            };

            var result = await _service.UpdatePromocaoAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(TipoDesconto.ValorFixo, promocao.TipoDesconto);
        }

        [Fact]
        public async Task UpdatePromocaoAsync_NomeNull_MantemOriginal()
        {
            var id = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocao = new Promocao
            {
                Id = id,
                Nome = "Nome Original",
                IdJogo = jogoId,
                Valor = 10m,
                TipoDesconto = TipoDesconto.Percentual,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10),
                Ativa = true
            };
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Jogo { Id = jogoId, Nome = "Jogo" });

            var dto = new UpdatePromocaoDTO
            {
                Nome = null,
                IdJogo = jogoId,
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 10m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10)
            };

            var result = await _service.UpdatePromocaoAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.Equal("Nome Original", promocao.Nome);
        }

        [Fact]
        public async Task UpdatePromocaoAsync_AtivaNull_MantemOriginal()
        {
            var id = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocao = new Promocao
            {
                Id = id,
                Nome = "Promo",
                IdJogo = jogoId,
                Valor = 10m,
                TipoDesconto = TipoDesconto.Percentual,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10),
                Ativa = true
            };
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Jogo { Id = jogoId, Nome = "Jogo" });

            var dto = new UpdatePromocaoDTO
            {
                Nome = "Promo",
                IdJogo = jogoId,
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 10m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10),
                Ativa = null
            };

            var result = await _service.UpdatePromocaoAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.True(promocao.Ativa);
        }

        [Fact]
        public async Task UpdatePromocaoAsync_AtivaFalse_AtualizaParaFalso()
        {
            var id = Guid.NewGuid();
            var jogoId = Guid.NewGuid();
            var promocao = new Promocao
            {
                Id = id,
                Nome = "Promo",
                IdJogo = jogoId,
                Valor = 10m,
                TipoDesconto = TipoDesconto.Percentual,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10),
                Ativa = true
            };
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(promocao);
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogoId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Jogo { Id = jogoId, Nome = "Jogo" });

            var dto = new UpdatePromocaoDTO
            {
                Nome = "Promo",
                IdJogo = jogoId,
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 10m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10),
                Ativa = false
            };

            var result = await _service.UpdatePromocaoAsync(id, dto);

            Assert.True(result.IsSuccess);
            Assert.False(promocao.Ativa);
        }

        [Fact]
        public async Task UpdatePromocaoAsync_DataFimIgualDataInicio_RetornaFalha_400()
        {
            var id = Guid.NewGuid();
            var agora = DateTime.UtcNow;
            _promocaoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Promocao { Id = id, Nome = "P", IdJogo = Guid.NewGuid() });

            var dto = new UpdatePromocaoDTO
            {
                Nome = "Test",
                IdJogo = Guid.NewGuid(),
                Valor = 10m,
                DataInicio = agora,
                DataFim = agora
            };

            var result = await _service.UpdatePromocaoAsync(id, dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(400, result.StatusCode);
        }

        #endregion

        #region Edge Cases - GetAll

        [Fact]
        public async Task GetAllPromocoesAsync_ListaVazia_RetornaSucessoSemDados()
        {
            _promocaoRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<Promocao>());

            var result = await _service.GetAllPromocoesAsync();

            Assert.True(result.IsSuccess);
            Assert.Empty(result.Data!);
        }

        #endregion
    }
}
