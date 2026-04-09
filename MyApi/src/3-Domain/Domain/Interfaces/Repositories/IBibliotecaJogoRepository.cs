using FCG.Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IBibliotecaJogoRepository : IRepository<BibliotecaJogo>
    {
        Task<IEnumerable<BibliotecaJogo>> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default);
    }
}
