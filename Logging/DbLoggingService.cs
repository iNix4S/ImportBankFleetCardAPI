using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ImportBankFleetCardAPI.Logging
{
    public class DbLoggingService : ILoggingService
    {
        private readonly string _connectionString;

        public DbLoggingService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("Oracle")
                ?? throw new InvalidOperationException("Oracle Connection String ('Oracle') is not configured in appsettings.json.");
        }

        public async Task LogErrorAsync(Exception ex, string message, string? contextInfo = null)
        {
            try
            {
                await using var connection = new OracleConnection(_connectionString);
                await using var command = new OracleCommand("FLEET_CARD_API_PKG.SP_INSERT_APP_LOG", connection)
                {
                    CommandType = CommandType.StoredProcedure
                };

                command.Parameters.Add("p_LogLevel", "ERROR");
                command.Parameters.Add("p_Message", OracleDbType.Varchar2, ex.Message, ParameterDirection.Input);
                command.Parameters.Add("p_StackTrace", OracleDbType.Clob, ex.ToString(), ParameterDirection.Input);
                command.Parameters.Add("p_ContextInfo", OracleDbType.Varchar2, $"{message} | Context: {contextInfo ?? "N/A"}", ParameterDirection.Input);

                await connection.OpenAsync();
                await command.ExecuteNonQueryAsync();
            }
            catch (Exception dbEx)
            {
                // หากการบันทึก Log ลงฐานข้อมูลล้มเหลว ให้แสดง Error ทั้งสองออกทาง Console แทน
                // เพื่อป้องกันไม่ให้แอปพลิเคชันหยุดทำงานทั้งหมด
                Console.WriteLine($"FATAL: Could not write to database log. DB-Error: {dbEx.Message}");
                Console.WriteLine($"Original-Error: {ex.Message}");
            }
        }
    }
}