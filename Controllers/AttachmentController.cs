using Microsoft.AspNetCore.Mvc;
using SecureFileAPI.Services;
using System.Threading.Tasks;

namespace SecureFileAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AttachmentController : ControllerBase
    {
        private readonly IDatabaseConnectionService _databaseConnectionService;

        public AttachmentController(IDatabaseConnectionService databaseConnectionService)
        {
            _databaseConnectionService = databaseConnectionService;
        }

        [HttpGet("test-connection")]
        public async Task<IActionResult> TestConnection()
        {
            bool isConnected = await _databaseConnectionService.TestConnection();
            if (isConnected)
            {
                return Ok("✅ Connected to database successfully!");
            }
            return StatusCode(500, "❌ Failed to connect to database.");
        }
    }
}
