
using System.Linq.Expressions;
using Dapper;
using Domain.Interfaces.Repositories;
using Infrastructure.Data.Context;
using System.Data;
using System.Text;

namespace Infrastructure.Data.Repositories
{
    public abstract class BaseRepository<T> : IRepository<T> where T : class
    {
        protected readonly DapperContext _context;
        protected readonly string _tableName;

        protected BaseRepository(DapperContext context)
        {
            _context = context;

            // Try to detect which table identifier exists in the database and use it.
            // Prefer unquoted lowercase (users). If not present and a quoted PascalCase table ("Users") exists,
            // use the quoted identifier so queries match.
            var lowerName = (typeof(T).Name + "s").ToLowerInvariant();
            var exactName = (typeof(T).Name + "s");

            try
            {
                using var connection = _context.CreateConnection();
                connection.Open();

                // Check for lowercase unquoted table
                var sql = "SELECT COUNT(1) FROM information_schema.tables WHERE table_schema = 'public' AND table_name = @name";
                var lowerExists = connection.ExecuteScalar<int>(sql, new { name = lowerName });
                if (lowerExists > 0)
                {
                    _tableName = lowerName;
                    return;
                }

                // Check for exact (case-sensitive) table name as stored in information_schema
                var exactExists = connection.ExecuteScalar<int>(sql, new { name = exactName });
                if (exactExists > 0)
                {
                    // Use quoted identifier so queries target the exact table name (e.g. "Users").
                    _tableName = $"\"{exactName}\"";
                    return;
                }
            }
            catch
            {
                // On any error, fall back to lowercase name to avoid breaking functionality.
            }

            // Default fallback
            _tableName = lowerName;
        }

        public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
{
    var properties = typeof(T).GetProperties()
        .Where(p => p.Name != "createdat");

    var columns = string.Join(", ", properties.Select(p => p.Name));
    var values = string.Join(", ", properties.Select(p => "@" + p.Name));

            var query = $"INSERT INTO {_tableName} ({columns}, createdat) VALUES ({values}, @createdat);";

            using var connection = _context.CreateConnection();
    await connection.ExecuteAsync(query, entity);
}

public virtual async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
{
    var query = $"UPDATE {_tableName} SET IsActive = 0 WHERE Id = @Id";
    using var connection = _context.CreateConnection();
    var rowsAffected = await connection.ExecuteAsync(query, new { Id = id });
    return rowsAffected > 0;
}

public virtual async Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
{
    var query = $"SELECT * FROM {_tableName} WHERE IsActive = 1";
    using var connection = _context.CreateConnection();
    return await connection.QueryAsync<T>(query);
}

public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
{
    var query = $"SELECT * FROM {_tableName} WHERE Id = @Id AND IsActive = 1";
    using var connection = _context.CreateConnection();
    return await connection.QuerySingleOrDefaultAsync<T>(query, new { Id = id });
}

public virtual async Task<bool> UpdateAsync(T entity, CancellationToken cancellationToken = default)
{
    var properties = typeof(T).GetProperties()
        .Where(p => p.Name != "Id" && p.Name != "CreatedAt");

    var setClause = string.Join(", ", properties.Select(p => $"{p.Name} = @{p.Name}"));
    var query = $"UPDATE {_tableName} SET {setClause}, UpdatedAt = @UpdatedAt WHERE Id = @Id";

    using var connection = _context.CreateConnection();
    var rowsAffected = await connection.ExecuteAsync(query, entity);
    return rowsAffected > 0;
}

public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
{
    using var connection = _context.CreateConnection();
    var query = $"SELECT * FROM {_tableName} WHERE IsActive = 1";
    return await connection.QueryAsync<T>(query);
}

public virtual async Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default)
{
    var query = $"SELECT COUNT(1) FROM {_tableName} WHERE Id = @Id AND IsActive = 1";
    using var connection = _context.CreateConnection();
    return await connection.ExecuteScalarAsync<bool>(query, new { Id = id });
}
    }
}
