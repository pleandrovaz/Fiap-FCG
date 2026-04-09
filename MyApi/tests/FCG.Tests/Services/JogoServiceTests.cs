using Application.DTOs;
using Application.Services;
using Domain.Interfaces.Repositories;
using FCG.Domain.Entities;
using Moq;

namespace FCG.Tests.Services
{
    public class JogoServiceTests
    {
        private readonly Mock<IJogoRepository> _jogoRepositoryMock;
        private readonly JogoService _service;

        public JogoServiceTests()
        {
            _jogoRepositoryMock = new Mock<IJogoRepository>();
            _service = new JogoService(_jogoRepositoryMock.Object);
        }

        #region GetAllJogosAsync

        [Fact]
        public async Task GetAllJogosAsync_ReturnTodosJogos()
        {
            var jogos = new List<Jogo>
            {
                new() { Id = Guid.NewGuid(), Nome = "Jogo1", Preco = 50m, Ativo = true },
                new() { Id = Guid.NewGuid(), Nome = "Jogo2", Preco = 100m, Ativo = false }
            };
            _jogoRepositoryMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(jogos);

            var result = await _service.GetAllJogosAsync();

            Assert.True(result.IsSuccess);
            Assert.Equal(2, result.Data!.Count());
        }

        #endregion

        #region GetJogoByIdAsync

        [Fact]
        public async Task GetJogoByIdAsync_Existe_ReturnSucesso()
        {
            var jogo = new Jogo { Id = Guid.NewGuid(), Nome = "Jogo1", Preco = 50m, Ativo = true };
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(jogo.Id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jogo);

            var result = await _service.GetJogoByIdAsync(jogo.Id);

            Assert.True(result.IsSuccess);
            Assert.Equal(jogo.Nome, result.Data!.Nome);
        }

        [Fact]
        public async Task GetJogoByIdAsync_NaoEncontrado_ReturnFalha_404()
        {
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Jogo?)null);

            var result = await _service.GetJogoByIdAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region CreateJogoAsync

        [Fact]
        public async Task CreateJogoAsync_ValidData_ReturnSucesso_201()
        {
            var dto = new CreateJogoDTO { Nome = "Novo Jogo", Descricao = "Desc", Preco = 59.99m };

            var result = await _service.CreateJogoAsync(dto);

            Assert.True(result.IsSuccess);
            Assert.Equal(201, result.StatusCode);
            Assert.Equal("Novo Jogo", result.Data!.Nome);
            _jogoRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Jogo>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        #endregion

        #region UpdateJogoAsync

        [Fact]
        public async Task UpdateJogoAsync_Existe_ReturnSucesso()
        {
            var id = Guid.NewGuid();
            var jogo = new Jogo { Id = id, Nome = "Old", Preco = 50m, Ativo = true };
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(jogo);

            var dto = new UpdateJogoDTO { Nome = "Updated", Descricao = "New Desc", Preco = 79.99m, Ativo = true };

            var result = await _service.UpdateJogoAsync(id, dto);

            Assert.True(result.IsSuccess);
            _jogoRepositoryMock.Verify(r => r.UpdateAsync(It.Is<Jogo>(j => j.Nome == "Updated"), It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task UpdateJogoAsync_NaoEncontrado_RetornoFalha_404()
        {
            _jogoRepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((Jogo?)null);

            var dto = new UpdateJogoDTO { Nome = "Test", Preco = 10m };

            var result = await _service.UpdateJogoAsync(Guid.NewGuid(), dto);

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion

        #region DeleteJogoAsync

        [Fact]
        public async Task DeleteJogoAsync_Existe_ReturnSucesso()
        {
            var id = Guid.NewGuid();
            _jogoRepositoryMock.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(true);

            var result = await _service.DeleteJogoAsync(id);

            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task DeleteJogoAsync_NaoEncontrado_RetornoFalha_404()
        {
            _jogoRepositoryMock.Setup(r => r.DeleteAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(false);

            var result = await _service.DeleteJogoAsync(Guid.NewGuid());

            Assert.False(result.IsSuccess);
            Assert.Equal(404, result.StatusCode);
        }

        #endregion
    }
}
