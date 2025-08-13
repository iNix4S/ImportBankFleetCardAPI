using Microsoft.AspNetCore.Mvc;
using ImportBankFleetCardAPI.Services;
using System.Threading.Tasks;
using System.Threading;

namespace ImportBankFleetCardAPI.Controllers
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
        public async Task<IActionResult> TestConnection(CancellationToken cancellationToken = default)
        {
            // หมายเหตุ: ต้องมีการปรับปรุงเมธอดใน Service (`IDatabaseConnectionService`) ให้รองรับ cancellation token ด้วย
            // การเรียกใช้เมธอด TestConnection โดยไม่มี CancellationToken เพื่อให้โค้ดคอมไพล์ผ่าน
            // หากต้องการใช้ CancellationToken อย่างสมบูรณ์ จะต้องไปแก้ไขที่ Interface และ Service ที่เกี่ยวข้อง
            bool isConnected = await _databaseConnectionService.TestConnection();
            if (isConnected)
            {
                return Ok("✅ Connected to database successfully!");
            }
            return StatusCode(500, "❌ Failed to connect to database.");
        }
    }
}
