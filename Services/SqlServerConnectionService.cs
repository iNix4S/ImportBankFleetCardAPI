using System;
using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;

namespace ImportBankFleetCardAPI.Services
{
        public class SqlServerConnectionService : IDatabaseConnectionService
        {
            public Task<Oracle.ManagedDataAccess.Client.OracleConnection> GetOpenConnectionAsync()
            {
                throw new NotImplementedException("GetOpenConnectionAsync is not implemented for SqlServerConnectionService.");
            }
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
