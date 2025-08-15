using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ImportBankFleetCardAPI.Repositories;
using ImportBankFleetCardAPI.Models;

namespace ImportBankFleetCardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionTestController : ControllerBase
    {
        private readonly IFleetCardRepository _repository;
        private readonly ImportBankFleetCardAPI.Services.IDatabaseConnectionService _oracleConnectionService;

        public TransactionTestController(IFleetCardRepository repository, ImportBankFleetCardAPI.Services.IDatabaseConnectionService oracleConnectionService)
        {
            _repository = repository;
            _oracleConnectionService = oracleConnectionService;
        }

        [HttpGet("test-insert-transaction")]
        public async Task<IActionResult> TestInsertTransaction()
        {
            var transaction = new FleetCardTransaction
            {
                CardNumber = "1234567890123456",
                PlateNumber = "TEST-PLATE",
                TransactionDate = System.DateTime.Now,
                StationName = "Test Station",
                ProductName = "Test Product",
                Quantity = 10,
                UnitPrice = 20,
                TotalAmount = 200,
                Status = "TEST"
            };
            try
            {
                await _repository.InsertTransactionAsync(transaction);
                return Ok(new { message = "Transaction inserted (if no error)." });
            }
            catch (System.Exception ex)
            {
                // Insert error log into FLEET_CARD_APP_LOGS
                try
                {
                    await using var logConn = await _oracleConnectionService.GetOpenConnectionAsync();
                    await using var logCmd = new Oracle.ManagedDataAccess.Client.OracleCommand(@"INSERT INTO FLEET_CARD_APP_LOGS (LOG_TIMESTAMP, LOG_LEVEL, MESSAGE, STACK_TRACE, CONTEXT_INFO)
                        VALUES (SYSTIMESTAMP, :logLevel, :message, :stackTrace, :contextInfo)", logConn)
                    {
                        CommandType = System.Data.CommandType.Text
                    };
                    logCmd.Parameters.Add(":logLevel", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 20).Value = "ERROR";
                    logCmd.Parameters.Add(":message", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 4000).Value = ex.Message;
                    logCmd.Parameters.Add(":stackTrace", Oracle.ManagedDataAccess.Client.OracleDbType.Clob).Value = ex.ToString();
                    logCmd.Parameters.Add(":contextInfo", Oracle.ManagedDataAccess.Client.OracleDbType.Varchar2, 1000).Value = $"TestInsertTransaction | CardNumber: {transaction.CardNumber}";
                    await logCmd.ExecuteNonQueryAsync();
                }
                catch (System.Exception logEx)
                {
                    System.Console.WriteLine($"[FATAL] Failed to write error log: {logEx.Message}");
                }
                return StatusCode(500, new { message = $"Failed to insert transaction: {ex.Message}" });
            }
        }
    }
}
