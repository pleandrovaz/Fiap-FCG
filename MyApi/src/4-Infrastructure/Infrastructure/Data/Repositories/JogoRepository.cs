using Domain.Interfaces.Repositories;
using FCG.Domain.Entities;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Data.Repositories
{
    public class JogoRepository : IJogoRepository
    {
        private readonly AppDbContext _context;

        public JogoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Jogo?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Jogos
                .FirstOrDefaultAsync(j => j.Id == id && j.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Jogo>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Jogos
                .Where(j => j.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Jogo>> FindAsync(Expression<Func<Jogo, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Jogos
                .Where(predicate)
                .Where(j => j.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Jogo entity, CancellationToken cancellationToken = default)
        {
            await _context.Jogos.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(Jogo entity, CancellationToken cancellationToken = default)
        {
            _context.Jogos.Update(entity);
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var jogo = await _context.Jogos.FindAsync(new object[] { id }, cancellationToken);
            if (jogo == null)
                return false;

            jogo.IsActive = false;
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Jogos
                .AnyAsync(j => j.Id == id && j.IsActive, cancellationToken);
        }
    }
}
