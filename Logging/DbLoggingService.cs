using Oracle.ManagedDataAccess.Client;
using System.Data;

namespace ImportBankFleetCardAPI.Logging
{
    public class DbLoggingService : ILoggingService
    {
        private readonly Services.IDatabaseConnectionService _oracleConnectionService;

        public DbLoggingService(Services.IDatabaseConnectionService oracleConnectionService)
        {
            _oracleConnectionService = oracleConnectionService;
        }

        public async Task LogErrorAsync(Exception ex, string message, string? contextInfo = null)
        {
            try
            {
                await using var connection = await _oracleConnectionService.GetOpenConnectionAsync();
                await using var command = new Oracle.ManagedDataAccess.Client.OracleCommand(@"INSERT INTO FLEET_CARD_APP_LOGS (LOG_TIMESTAMP, LOG_LEVEL, MESSAGE, STACK_TRACE, CONTEXT_INFO)
                    VALUES (SYSTIMESTAMP, :logLevel, :message, :stackTrace, :contextInfo)", connection)
                {
                    CommandType = System.Data.CommandType.Text
                };

                command.Parameters.Add(":logLevel", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 20).Value = "ERROR";
                command.Parameters.Add(":message", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 4000).Value = ex.Message;
                command.Parameters.Add(":stackTrace", Oracle.ManagedDataAccess.Client.OracleDbType.Clob).Value = ex.ToString();
                command.Parameters.Add(":contextInfo", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 1000).Value = $"{message} | Context: {contextInfo ?? "N/A"}";

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