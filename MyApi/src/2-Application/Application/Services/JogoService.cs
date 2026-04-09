using Application.DTOs;
using Application.Interfaces;
using Application.Resources;
using Domain.Interfaces.Repositories;
using FCG.Domain.Entities;

namespace Application.Services
{
    public class JogoService : IJogoService
    {
        private readonly IJogoRepository _jogoRepository;

        public JogoService(IJogoRepository jogoRepository)
        {
            _jogoRepository = jogoRepository;
        }

        public async Task<Result<IEnumerable<JogoDTO>>> GetAllJogosAsync(CancellationToken cancellationToken = default)
        {
            var jogos = await _jogoRepository.GetAllAsync(cancellationToken);

            var result = jogos.Select(j => new JogoDTO
            {
                Id = j.Id,
                Nome = j.Nome,
                Descricao = j.Descricao,
                Preco = j.Preco,
                Ativo = j.Ativo
            });

            return Result<IEnumerable<JogoDTO>>.Success(result);
        }

        public async Task<Result<JogoDTO>> GetJogoByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var jogo = await _jogoRepository.GetByIdAsync(id, cancellationToken);

            if (jogo == null)
                return Result<JogoDTO>.Failure(Messages.JogoNaoEncontrado, 404);

            return Result<JogoDTO>.Success(new JogoDTO
            {
                Id = jogo.Id,
                Nome = jogo.Nome,
                Descricao = jogo.Descricao,
                Preco = jogo.Preco,
                Ativo = jogo.Ativo
            });
        }

        public async Task<Result<JogoDTO>> CreateJogoAsync(CreateJogoDTO dto, CancellationToken cancellationToken = default)
        {
            var jogo = new Jogo
            {
                Id = Guid.NewGuid(),
                Nome = dto.Nome,
                Descricao = dto.Descricao,
                Preco = dto.Preco,
                Ativo = true,
                CreatedAt = DateTime.UtcNow,
                IsActive = true
            };

            await _jogoRepository.AddAsync(jogo, cancellationToken);

            return Result<JogoDTO>.Success(new JogoDTO
            {
                Id = jogo.Id,
                Nome = jogo.Nome,
                Descricao = jogo.Descricao,
                Preco = jogo.Preco,
                Ativo = jogo.Ativo
            }, 201);
        }

        public async Task<Result<bool>> UpdateJogoAsync(Guid id, UpdateJogoDTO dto, CancellationToken cancellationToken = default)
        {
            var jogo = await _jogoRepository.GetByIdAsync(id, cancellationToken);
            if (jogo == null)
                return Result<bool>.Failure(Messages.JogoNaoEncontrado, 404);

            jogo.Nome = dto.Nome;
            jogo.Descricao = dto.Descricao;
            jogo.Preco = dto.Preco;
            jogo.Ativo = dto.Ativo;
            jogo.UpdatedAt = DateTime.UtcNow;

            await _jogoRepository.UpdateAsync(jogo, cancellationToken);
            return Result<bool>.Success(true);
        }

        public async Task<Result<bool>> DeleteJogoAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var deleted = await _jogoRepository.DeleteAsync(id, cancellationToken);
            if (!deleted)
                return Result<bool>.Failure(Messages.JogoNaoEncontrado, 404);

            return Result<bool>.Success(true);
        }
    }
}
