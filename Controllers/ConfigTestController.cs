using Microsoft.AspNetCore.Mvc;
using ImportBankFleetCardAPI.Config;
using ImportBankFleetCardAPI.Services;
using System.Threading.Tasks;
using Oracle.ManagedDataAccess.Client;

namespace ImportBankFleetCardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ConfigTestController : ControllerBase
    {
        private readonly IConfigService _configService;
        private readonly IDatabaseConnectionService _oracleConnectionService;

        public ConfigTestController(IConfigService configService, IDatabaseConnectionService oracleConnectionService)
        {
            _configService = configService;
            _oracleConnectionService = oracleConnectionService;
        }

        [HttpGet("test-get-import-configs")]
        public async Task<IActionResult> TestGetImportConfigs()
        {
            List<Dictionary<string, object>> configs = new();
            string logMessage;
            string logError = null;
            try
            {
                await using var conn = await _oracleConnectionService.GetOpenConnectionAsync();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = @"SELECT CONFIG_ID, TEMPLATE_NAME, FIELD_NAME, SOURCE_COLUMN_NAME, SOURCE_COLUMN_INDEX, IS_REQUIRED
                                    FROM FLEET_CARD_IMPORT_CONFIGS";
                using var reader = await cmd.ExecuteReaderAsync();

                while (await reader.ReadAsync())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = await reader.IsDBNullAsync(i) ? null : reader.GetValue(i);
                    }
                    configs.Add(row);
                }

                logMessage = $"SUCCESS: Get {configs.Count} rows from FLEET_CARD_IMPORT_CONFIGS";
            }
            catch (Exception ex)
            {
                logMessage = $"ERROR: {ex.Message}";
            }

            // Insert log to FLEET_CARD_APP_LOGS
            try
            {
                await using var logConn = await _oracleConnectionService.GetOpenConnectionAsync();
                using var logCmd = logConn.CreateCommand();
                logCmd.CommandText = @"
                    INSERT INTO FLEET_CARD_APP_LOGS (LOG_TIMESTAMP, LOG_LEVEL, MESSAGE, STACK_TRACE, CONTEXT_INFO)
                    VALUES (SYSTIMESTAMP, :level, :msg, :stack, :ctx)";
                logCmd.Parameters.Add(new OracleParameter("level", "INFO"));
                logCmd.Parameters.Add(new OracleParameter("msg", logMessage));
                logCmd.Parameters.Add(new OracleParameter("stack", DBNull.Value));
                logCmd.Parameters.Add(new OracleParameter("ctx", System.Text.Json.JsonSerializer.Serialize(configs)));
                await logCmd.ExecuteNonQueryAsync();
            }
            catch (Exception logEx)
            {
                logError = $"ERROR insert log: {logEx.Message}";
            }

            return Ok(new { message = logMessage, logError, configs });
        }
    }
}