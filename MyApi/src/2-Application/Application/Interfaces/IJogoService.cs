using Application.DTOs;

namespace Application.Interfaces
{
    public interface IJogoService
    {
        Task<Result<IEnumerable<JogoDTO>>> GetAllJogosAsync(CancellationToken cancellationToken = default);
        Task<Result<JogoDTO>> GetJogoByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<JogoDTO>> CreateJogoAsync(CreateJogoDTO dto, CancellationToken cancellationToken = default);
        Task<Result<bool>> UpdateJogoAsync(Guid id, UpdateJogoDTO dto, CancellationToken cancellationToken = default);
        Task<Result<bool>> DeleteJogoAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
