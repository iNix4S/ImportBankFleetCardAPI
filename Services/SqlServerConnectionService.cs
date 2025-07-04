using System;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace SecureFileAPI.Services
{
    public class SqlServerConnectionService : IDatabaseConnectionService
    {
        private readonly string _connectionString;

        public SqlServerConnectionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("SqlServer") ?? throw new ArgumentNullException(nameof(configuration));
        }


        public async Task<bool> TestConnection()
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("✅ SqlServer Connection Success!");
                    return true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ SqlServer Connection Error: {ex.Message}");
                return false;
            }
        }
    }
}
