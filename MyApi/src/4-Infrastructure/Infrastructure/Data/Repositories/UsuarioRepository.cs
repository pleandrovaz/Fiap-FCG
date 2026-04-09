using Domain.Entities;
using Domain.Interfaces.Repositories;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Data.Repositories
{
    public class UsuarioRepository : IUsuarioRepository
    {
        private readonly AppDbContext _context;

        public UsuarioRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Usuario?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Id == id && u.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Usuario>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .Where(u => u.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Usuario>> FindAsync(Expression<Func<Usuario, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .Where(predicate)
                .Where(u => u.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            await _context.Usuarios.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(Usuario entity, CancellationToken cancellationToken = default)
        {
            _context.Usuarios.Update(entity);
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var usuario = await _context.Usuarios.FindAsync(new object[] { id }, cancellationToken);
            if (usuario == null)
                return false;

            usuario.IsActive = false;
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .AnyAsync(u => u.Id == id && u.IsActive, cancellationToken);
        }

        public async Task<Usuario?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive, cancellationToken);
        }

        public async Task<Usuario?> GetByEmailAndPasswordAsync(string email, string passwordHash, CancellationToken cancellationToken = default)
        {
            return await _context.Usuarios
                .FirstOrDefaultAsync(u => u.Email == email && u.PasswordHash == passwordHash && u.IsActive, cancellationToken);
        }
    }
}
