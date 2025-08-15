using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ImportBankFleetCardAPI.Logging;

namespace ImportBankFleetCardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LogTestController : ControllerBase
    {
        private readonly ILoggingService _loggingService;

        public LogTestController(ILoggingService loggingService)
        {
            _loggingService = loggingService;
        }

        [HttpGet("test-db-log")]
        public async Task<IActionResult> TestDbLog()
        {
            try
            {
                await _loggingService.LogErrorAsync(new Exception("TestException"), "Test log from /api/logtest/test-db-log endpoint.", "Manual test context");
                return Ok(new { message = "Log written to DB (if no error)." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Failed to write log: {ex.Message}" });
            }
        }
    }
}
