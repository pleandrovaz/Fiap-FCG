using API.Controllers;
using Application.DTOs;
using Application.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace FCG.Tests.TDD.Controllers
{
    public class PromocoesControllerTests
    {
        private readonly Mock<IPromocaoService> _promocaoServiceMock;
        private readonly PromocoesController _controller;

        public PromocoesControllerTests()
        {
            _promocaoServiceMock = new Mock<IPromocaoService>();
            _controller = new PromocoesController(_promocaoServiceMock.Object);
        }

        #region GetAllPromocoes

        [Fact]
        public async Task GetAllPromocoes_Sucesso_RetornaOkComDados()
        {
            var promocoes = new List<PromocaoDTO>
            {
                new() { Id = Guid.NewGuid(), Nome = "Promo1", Valor = 10m, Ativa = true },
                new() { Id = Guid.NewGuid(), Nome = "Promo2", Valor = 5m, Ativa = false }
            };
            _promocaoServiceMock
                .Setup(s => s.GetAllPromocoesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<IEnumerable<PromocaoDTO>>.Success(promocoes));

            var result = await _controller.TodasPromocoes(CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsAssignableFrom<IEnumerable<PromocaoDTO>>(okResult.Value);
            Assert.Equal(2, data.Count());
        }

        [Fact]
        public async Task GetAllPromocoes_Falha_RetornaStatusCodeComMensagem()
        {
            _promocaoServiceMock
                .Setup(s => s.GetAllPromocoesAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<IEnumerable<PromocaoDTO>>.Failure("Erro interno", 500));

            var result = await _controller.TodasPromocoes(CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        #endregion

        #region GetPromocaoById

        [Fact]
        public async Task GetPromocaoById_Encontrado_RetornaOkComDados()
        {
            var id = Guid.NewGuid();
            var dto = new PromocaoDTO { Id = id, Nome = "Promo", Valor = 15m, Ativa = true };
            _promocaoServiceMock
                .Setup(s => s.GetPromocaoByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PromocaoDTO>.Success(dto));

            var result = await _controller.GetPromocaoById(id, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            var data = Assert.IsType<PromocaoDTO>(okResult.Value);
            Assert.Equal(id, data.Id);
        }

        [Fact]
        public async Task GetPromocaoById_NaoEncontrado_Retorna404()
        {
            var id = Guid.NewGuid();
            _promocaoServiceMock
                .Setup(s => s.GetPromocaoByIdAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PromocaoDTO>.Failure("Promoção não encontrada", 404));

            var result = await _controller.GetPromocaoById(id, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        #endregion

        #region CreatePromocao

        [Fact]
        public async Task CreatePromocao_Sucesso_RetornaCreatedAtAction201()
        {
            var jogoId = Guid.NewGuid();
            var createDto = new CreatePromocaoDTO
            {
                Nome = "Nova Promo",
                IdJogo = jogoId,
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 20m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(30)
            };
            var resultDto = new PromocaoDTO
            {
                Id = Guid.NewGuid(),
                Nome = "Nova Promo",
                IdJogo = jogoId,
                TipoDesconto = TipoDesconto.Percentual,
                Valor = 20m,
                Ativa = true
            };
            _promocaoServiceMock
                .Setup(s => s.CreatePromocaoAsync(createDto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PromocaoDTO>.Success(resultDto, 201));

            var result = await _controller.AdicionarPromocao(createDto, CancellationToken.None);

            var createdResult = Assert.IsType<CreatedAtActionResult>(result.Result);
            Assert.Equal(201, createdResult.StatusCode);
            var data = Assert.IsType<PromocaoDTO>(createdResult.Value);
            Assert.Equal(resultDto.Id, data.Id);
        }

        [Fact]
        public async Task CreatePromocao_DataInvalida_Retorna400()
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
            _promocaoServiceMock
                .Setup(s => s.CreatePromocaoAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PromocaoDTO>.Failure("Data fim deve ser após data início", 400));

            var result = await _controller.AdicionarPromocao(dto, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        [Fact]
        public async Task CreatePromocao_JogoNaoEncontrado_Retorna404()
        {
            var dto = new CreatePromocaoDTO
            {
                Nome = "Promo",
                IdJogo = Guid.NewGuid(),
                TipoDesconto = TipoDesconto.ValorFixo,
                Valor = 10m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10)
            };
            _promocaoServiceMock
                .Setup(s => s.CreatePromocaoAsync(dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<PromocaoDTO>.Failure("Jogo não encontrado", 404));

            var result = await _controller.AdicionarPromocao(dto, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        #endregion

        #region UpdatePromocao

        [Fact]
        public async Task UpdatePromocao_Sucesso_RetornaNoContent()
        {
            var id = Guid.NewGuid();
            var dto = new UpdatePromocaoDTO
            {
                Nome = "Updated",
                IdJogo = Guid.NewGuid(),
                TipoDesconto = TipoDesconto.ValorFixo,
                Valor = 25m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(20)
            };
            _promocaoServiceMock
                .Setup(s => s.UpdatePromocaoAsync(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.AtualizarPromocao(id, dto, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task UpdatePromocao_NaoEncontrado_Retorna404()
        {
            var id = Guid.NewGuid();
            var dto = new UpdatePromocaoDTO
            {
                Nome = "Test",
                IdJogo = Guid.NewGuid(),
                Valor = 10m,
                DataInicio = DateTime.UtcNow,
                DataFim = DateTime.UtcNow.AddDays(10)
            };
            _promocaoServiceMock
                .Setup(s => s.UpdatePromocaoAsync(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("Promoção não encontrada", 404));

            var result = await _controller.AtualizarPromocao(id, dto, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        [Fact]
        public async Task UpdatePromocao_DataInvalida_Retorna400()
        {
            var id = Guid.NewGuid();
            var dto = new UpdatePromocaoDTO
            {
                Nome = "Test",
                IdJogo = Guid.NewGuid(),
                Valor = 10m,
                DataInicio = DateTime.UtcNow.AddDays(10),
                DataFim = DateTime.UtcNow
            };
            _promocaoServiceMock
                .Setup(s => s.UpdatePromocaoAsync(id, dto, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("Data fim deve ser após data início", 400));

            var result = await _controller.AtualizarPromocao(id, dto, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(400, objectResult.StatusCode);
        }

        #endregion

        #region DeletePromocao

        [Fact]
        public async Task DeletePromocao_Sucesso_RetornaNoContent()
        {
            var id = Guid.NewGuid();
            _promocaoServiceMock
                .Setup(s => s.DeletePromocaoAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Success(true));

            var result = await _controller.DeletePromocao(id, CancellationToken.None);

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeletePromocao_NaoEncontrado_Retorna404()
        {
            var id = Guid.NewGuid();
            _promocaoServiceMock
                .Setup(s => s.DeletePromocaoAsync(id, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Result<bool>.Failure("Promoção não encontrada", 404));

            var result = await _controller.DeletePromocao(id, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(404, objectResult.StatusCode);
        }

        #endregion
    }
}
