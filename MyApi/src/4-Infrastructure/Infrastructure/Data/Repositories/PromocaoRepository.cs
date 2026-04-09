using Domain.Interfaces.Repositories;
using FCG.Domain.Entities;
using Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace Infrastructure.Data.Repositories
{
    public class PromocaoRepository : IPromocaoRepository
    {
        private readonly AppDbContext _context;

        public PromocaoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Promocao?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Promocoes
                .Include(p => p.Jogo)
                .FirstOrDefaultAsync(p => p.Id == id && p.IsActive, cancellationToken);
        }

        public async Task<IEnumerable<Promocao>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Promocoes
                .Include(p => p.Jogo)
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task<IEnumerable<Promocao>> FindAsync(Expression<Func<Promocao, bool>> predicate, CancellationToken cancellationToken = default)
        {
            return await _context.Promocoes
                .Where(predicate)
                .Where(p => p.IsActive)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(Promocao entity, CancellationToken cancellationToken = default)
        {
            await _context.Promocoes.AddAsync(entity, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
        }

        public async Task<bool> UpdateAsync(Promocao entity, CancellationToken cancellationToken = default)
        {
            _context.Promocoes.Update(entity);
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
        {
            var promocao = await _context.Promocoes.FindAsync(new object[] { id }, cancellationToken);
            if (promocao == null)
                return false;

            promocao.IsActive = false;
            var rowsAffected = await _context.SaveChangesAsync(cancellationToken);
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Promocoes
                .AnyAsync(p => p.Id == id && p.IsActive, cancellationToken);
        }
    }
}
