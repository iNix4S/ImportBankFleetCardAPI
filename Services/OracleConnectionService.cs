using System;
using System.Data;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Oracle.ManagedDataAccess.Client;

// namespace SecureFileAPI.Services
// {
//     public class OracleConnectionService : IDatabaseConnectionService
//     {
//         private readonly string _connectionString;

//         public OracleConnectionService(IConfiguration configuration)
//         {
//             _connectionString = configuration.GetConnectionString("Oracle") 
//                 ?? throw new ArgumentNullException(nameof(configuration), "Oracle connection string is missing.");
//         }

//         public async Task<bool> TestConnection()
//         {
//             try
//             {
//                 return await Task.Run(() =>
//                 {
//                     using var connection = new OracleConnection(_connectionString);
//                     connection.Open();  // ✅ ใช้ Open() เพราะ IDbConnection ไม่มี OpenAsync()

//                     using var command = connection.CreateCommand();
//                     command.CommandText = "SELECT 1 FROM DUAL";  // ✅ คำสั่ง SQL สำหรับ Oracle

//                     var result = command.ExecuteScalar();
//                     return result != null;
//                 });
//             }
//             catch (OracleException ex)  // ✅ จับ Oracle Exception โดยเฉพาะ
//             {
//                 Console.WriteLine($"❌ Oracle Connection Error: {ex.Message}");
//                 return false;
//             }
//             catch (Exception ex)
//             {
//                 Console.WriteLine($"❌ General Error: {ex.Message}");
//                 return false;
//             }
//         }
//     }
// }

namespace SecureFileAPI.Services
{
    public class OracleConnectionService : IDatabaseConnectionService
    {
        private readonly string _connectionString;

        public OracleConnectionService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Oracle") 
                ?? throw new ArgumentNullException(nameof(configuration), "Oracle connection string is missing.");
        }

        public async Task<bool> TestConnection()
        {
            try
            {
                using var connection = new OracleConnection(_connectionString);
                await connection.OpenAsync();  // ✅ ใช้ OpenAsync() เพื่อรองรับ async

                using var command = connection.CreateCommand();
                command.CommandText = "SELECT 1 FROM DUAL";  // ✅ SQL ตรวจสอบการเชื่อมต่อ Oracle

                var result = await command.ExecuteScalarAsync();
                Console.WriteLine("✅ Oracle Connection Success!");
                return result != null;
            }
            catch (OracleException ex)  // ✅ ดักจับเฉพาะ Oracle Exception
            {
                Console.WriteLine($"❌ Oracle Connection Error: {ex.Message}");
                return false;
            }
            catch (Exception ex)  // ✅ ดักจับ General Exception
            {
                Console.WriteLine($"❌ General Error: {ex.Message}");
                return false;
            }
        }
    }
}