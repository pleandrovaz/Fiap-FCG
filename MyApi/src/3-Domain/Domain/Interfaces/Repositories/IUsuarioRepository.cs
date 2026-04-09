using Domain.Entities;

namespace Domain.Interfaces.Repositories
{
    public interface IUsuarioRepository : IRepository<Usuario>
    {
        Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
        Task<Usuario?> GetByEmailAndPasswordAsync(string email, string passwordHash, CancellationToken cancellationToken = default);
    }
}
