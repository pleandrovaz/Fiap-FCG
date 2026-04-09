using Application.DTOs;
using Application.Interfaces;
using Application.Resources;
using Domain.Interfaces.Repositories;
using FCG.Domain.Entities;

namespace Application.Services
{
    public class BibliotecaJogoService : IBibliotecaJogoService
    {
        private readonly IBibliotecaJogoRepository _bibliotecaRepository;
        private readonly IJogoRepository _jogoRepository;
        private readonly IPromocaoRepository _promocaoRepository;

        public BibliotecaJogoService(
            IBibliotecaJogoRepository bibliotecaRepository,
            IJogoRepository jogoRepository,
            IPromocaoRepository promocaoRepository)
        {
            _bibliotecaRepository = bibliotecaRepository;
            _jogoRepository = jogoRepository;
            _promocaoRepository = promocaoRepository;
        }

        public async Task<Result<IEnumerable<BibliotecaJogoDTO>>> GetBibliotecaByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        {
            var itens = await _bibliotecaRepository.GetByUsuarioIdAsync(usuarioId, cancellationToken);

            var result = itens.Select(b => new BibliotecaJogoDTO
            {
                Id = b.Id,
                UsuarioId = b.UsuarioId,
                JogoId = b.JogoId,
                JogoNome = b.Jogo?.Nome,
                DataCompra = b.DataCompra,
                PrecoPago = b.PrecoPago,
                PromocaoId = b.PromocaoId
            });

            return Result<IEnumerable<BibliotecaJogoDTO>>.Success(result);
        }

        public async Task<Result<BibliotecaJogoDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _bibliotecaRepository.GetByIdAsync(id, cancellationToken);

            if (item == null)
                return Result<BibliotecaJogoDTO>.Failure(Messages.ItemNaoEncontrado, 404);

            return Result<BibliotecaJogoDTO>.Success(new BibliotecaJogoDTO
            {
                Id = item.Id,
                UsuarioId = item.UsuarioId,
                JogoId = item.JogoId,
                JogoNome = item.Jogo?.Nome,
                DataCompra = item.DataCompra,
                PrecoPago = item.PrecoPago,
                PromocaoId = item.PromocaoId
            });
        }

        public async Task<Result<BibliotecaJogoDTO>> AddJogoToBibliotecaAsync(Guid usuarioId, CreateBibliotecaJogoDTO dto, CancellationToken cancellationToken = default)
        {
            BibliotecaJogo? jogoExistente = (await _bibliotecaRepository.FindAsync(b => b.UsuarioId == usuarioId && b.JogoId == dto.JogoId, cancellationToken)).FirstOrDefault();
                if (jogoExistente != null)
                    return Result<BibliotecaJogoDTO>.Failure(Messages.JogoJaCadastradoNaBiblioteca, 409);

            var jogo = await _jogoRepository.GetByIdAsync(dto.JogoId, cancellationToken);
            if (jogo == null)
                return Result<BibliotecaJogoDTO>.Failure(Messages.JogoNaoEncontrado, 404);

            if (!jogo.Ativo)
                return Result<BibliotecaJogoDTO>.Failure(Messages.JogoInativo, 400);

            decimal precoPago = jogo.Preco;

            if (dto.PromocaoId.HasValue)
            {
                var promocao = await _promocaoRepository.GetByIdAsync(dto.PromocaoId.Value, cancellationToken);
                if (promocao == null || !promocao.Ativa || DateTime.UtcNow < promocao.DataInicio || DateTime.UtcNow > promocao.DataFim)
                    return Result<BibliotecaJogoDTO>.Failure(Messages.PromocaoInvalidaOuExpirada, 400);

                precoPago = promocao.TipoDesconto == Domain.Enums.TipoDesconto.Percentual
                    ? jogo.Preco - (jogo.Preco * promocao.Valor / 100)
                    : jogo.Preco - promocao.Valor;

                if (precoPago < 0) precoPago = 0;
            }

            var bibliotecaJogo = new BibliotecaJogo
            {
                Id = Guid.NewGuid(),
                UsuarioId = usuarioId,
                JogoId = dto.JogoId,
                DataCompra = DateTime.UtcNow,
                PrecoPago = precoPago,
                PromocaoId = dto.PromocaoId,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _bibliotecaRepository.AddAsync(bibliotecaJogo, cancellationToken);

            return Result<BibliotecaJogoDTO>.Success(new BibliotecaJogoDTO
            {
                Id = bibliotecaJogo.Id,
                UsuarioId = bibliotecaJogo.UsuarioId,
                JogoId = bibliotecaJogo.JogoId,
                JogoNome = jogo.Nome,
                DataCompra = bibliotecaJogo.DataCompra,
                PrecoPago = bibliotecaJogo.PrecoPago,
                PromocaoId = bibliotecaJogo.PromocaoId
            }, 201);
        }

        public async Task<Result<bool>> RemoveJogoFromBibliotecaAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var deleted = await _bibliotecaRepository.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return Result<bool>.Failure(Messages.ItemNaoEncontrado, 404);

            return Result<bool>.Success(true);
        }
    }
}
