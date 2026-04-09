using Domain.Interfaces.Repositories;
using FCG.Domain.Entities;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Data.Repositories
{
    public class BibliotecaJogoRepository : IBibliotecaJogoRepository
    {
        private readonly AppDbContext _context;

        public BibliotecaJogoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<BibliotecaJogo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.BibliotecaJogos
                .Include(b => b.Jogo)
                .FirstOrDefaultAsync(b => b.Id == id && b.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<BibliotecaJogo>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.BibliotecaJogos
                .Include(b => b.Jogo)
                .Where(b => b.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<BibliotecaJogo>> GetByUsuarioIdAsync(Guid usuarioId, CancellationToken cancellationToken = default)
        {
            return await _context.BibliotecaJogos
                .Include(b => b.Jogo)
                .Where(b => b.UsuarioId == usuarioId && b.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<BibliotecaJogo>> FindAsync(Expression<Func<BibliotecaJogo, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.BibliotecaJogos
                .Include(b => b.Jogo)
                .Where(predicate)
                .Where(b => b.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(BibliotecaJogo entity, CancellationToken cancellationToken = default)
        {
            await _context.BibliotecaJogos.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(BibliotecaJogo entity, CancellationToken cancellationToken = default)
        {
            _context.BibliotecaJogos.Update(entity);
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var item = await _context.BibliotecaJogos.FindAsync(new object[] { id }, cancellationToken);
            if (item == null)
                return false;

            item.IsActive = false;
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.BibliotecaJogos
                .AnyAsync(b => b.Id == id && b.IsActive, cancellationToken);
        }
    }
}
