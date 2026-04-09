// Infrastructure/Data/Context/DapperContext.cs
using Microsoft.Data.SqlClient;
using Npgsql;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace Infrastructure.Data.Context
{
    public class DapperContext
    {
        private readonly IConfiguration _configuration;
        private readonly string _connectionString;
        private readonly string _provider;

        public DapperContext(IConfiguration configuration)
        {
            _configuration = configuration;
            _connectionString = _configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
            _provider = _configuration["DatabaseProvider"] ?? string.Empty;
        }

        public IDbConnection CreateConnection()
        {
            if (!string.IsNullOrEmpty(_provider) && (_provider.Equals("postgres", System.StringComparison.OrdinalIgnoreCase)
                || _provider.Equals("npgsql", System.StringComparison.OrdinalIgnoreCase)
                || _provider.Contains("post", System.StringComparison.OrdinalIgnoreCase)))
            {
                return new NpgsqlConnection(_connectionString);
            }

            return new SqlConnection(_connectionString);
        }
    }
}
