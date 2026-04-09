using Application.DTOs;
using Application.Interfaces;
using Application.Resources;
using Domain.Interfaces.Repositories;
using FCG.Domain.Entities;

namespace Application.Services
{
    public class PromocaoService : IPromocaoService
    {
        private readonly IPromocaoRepository _promocaoRepository;
        private readonly IJogoRepository _jogoRepository;

        public PromocaoService(IPromocaoRepository promocaoRepository, IJogoRepository jogoRepository)
        {
            _promocaoRepository = promocaoRepository;
            _jogoRepository = jogoRepository;
        }

        public async Task<Result<IEnumerable<PromocaoDTO>>> GetAllPromocoesAsync(CancellationToken cancellationToken = default)
        {
            var promocoes = await _promocaoRepository.GetAllAsync(cancellationToken);

            var result = promocoes.Select(p => new PromocaoDTO
            {
                Id = p.Id,
                Nome = p.Nome,
                IdJogo = p.IdJogo,
                JogoNome = p.Jogo?.Nome,
                TipoDesconto = p.TipoDesconto,
                Valor = p.Valor,
                DataInicio = p.DataInicio,
                DataFim = p.DataFim,
                Ativa = p.Ativa
            });

            return Result<IEnumerable<PromocaoDTO>>.Success(result);
        }

        public async Task<Result<PromocaoDTO>> GetPromocaoByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var promocao = await _promocaoRepository.GetByIdAsync(id, cancellationToken);

            if (promocao == null)
                return Result<PromocaoDTO>.Failure(Messages.PromocaoNaoEncontrada, 404);

            return Result<PromocaoDTO>.Success(new PromocaoDTO
            {
                Id = promocao.Id,
                Nome = promocao.Nome,
                IdJogo = promocao.IdJogo,
                JogoNome = promocao.Jogo?.Nome,
                TipoDesconto = promocao.TipoDesconto,
                Valor = promocao.Valor,
                DataInicio = promocao.DataInicio,
                DataFim = promocao.DataFim,
                Ativa = promocao.Ativa
            });
        }

        public async Task<Result<PromocaoDTO>> CreatePromocaoAsync(CreatePromocaoDTO dto, CancellationToken cancellationToken = default)
        {
            if (dto.DataFim <= dto.DataInicio)
                return Result<PromocaoDTO>.Failure(Messages.DataFimDeveSerAposDataInicio, 400);

            var jogo = await _jogoRepository.GetByIdAsync(dto.IdJogo, cancellationToken);
            if (jogo == null)
                return Result<PromocaoDTO>.Failure(Messages.JogoNaoEncontrado, 404);

            var promocao = new Promocao
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                IdJogo = dto.IdJogo,
                TipoDesconto = dto.TipoDesconto,
                Valor = dto.Valor,
                DataInicio = dto.DataInicio,
                DataFim = dto.DataFim,
                Ativa = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _promocaoRepository.AddAsync(promocao, cancellationToken);

            return Result<PromocaoDTO>.Success(new PromocaoDTO
            {
                Id = promocao.Id,
                Nome = promocao.Nome,
                IdJogo = promocao.IdJogo,
                JogoNome = jogo.Nome,
                TipoDesconto = promocao.TipoDesconto,
                Valor = promocao.Valor,
                DataInicio = promocao.DataInicio,
                DataFim = promocao.DataFim,
                Ativa = promocao.Ativa
            }, 201);
        }

        public async Task<Result<bool>> UpdatePromocaoAsync(Guid id, UpdatePromocaoDTO dto, CancellationToken cancellationToken = default)
        {
            var promocao = await _promocaoRepository.GetByIdAsync(id, cancellationToken);
            if (promocao == null)
                return Result<bool>.Failure(Messages.PromocaoNaoEncontrada, 404);

            if (dto.DataFim <= dto.DataInicio)
                return Result<bool>.Failure(Messages.DataFimDeveSerAposDataInicio, 400);

            var jogo = await _jogoRepository.GetByIdAsync(dto.IdJogo, cancellationToken);
            if (jogo == null)
                return Result<bool>.Failure(Messages.JogoNaoEncontrado, 404);

            promocao.Nome =dto.Nome?? promocao.Nome;
            promocao.IdJogo =dto.IdJogo;
            promocao.TipoDesconto =dto.TipoDesconto==0 ? promocao.TipoDesconto : dto.TipoDesconto;
            promocao.Valor = dto.Valor <= 0.0m ? promocao.Valor : dto.Valor;
            promocao.DataInicio = promocao.DataInicio;
            promocao.DataFim = dto.DataFim;
            promocao.Ativa = dto.Ativa ?? promocao.Ativa;
            promocao.UpdatedAt = DateTime.UtcNow;

            await _promocaoRepository.UpdateAsync(promocao, cancellationToken);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeletePromocaoAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var deleted = await _promocaoRepository.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return Result<bool>.Failure(Messages.PromocaoNaoEncontrada, 404);

            return Result<bool>.Success(true);
        }
    }
}
