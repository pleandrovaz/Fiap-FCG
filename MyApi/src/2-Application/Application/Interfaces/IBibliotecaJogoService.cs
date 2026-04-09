using Application.DTOs;

namespace Application.Interfaces
{
    public interface IBibliotecaJogoService
    {
        Task<Result<IEnumerable<BibliotecaJogoDTO>>> GetBibliotecaByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
        Task<Result<BibliotecaJogoDTO>> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<Result<BibliotecaJogoDTO>> AddJogoToBibliotecaAsync(Guid usuarioId, CreateBibliotecaJogoDTO dto, CancellationToken cancellationToken = default);
        Task<Result<bool>> RemoveJogoFromBibliotecaAsync(Guid id, CancellationToken cancellationToken = default);
    }
}
