using ImportBankFleetCardAPI.DTOs;
using ImportBankFleetCardAPI.Logging;
using ImportBankFleetCardAPI.Models;
using ImportBankFleetCardAPI.Repositories;
using ImportBankFleetCardAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace ImportBankFleetCardAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class FleetCardController : ControllerBase
    {
        private readonly IImportService _importService;
        private readonly IFleetCardRepository _repository;
        private readonly ILoggingService _loggingService;

        public FleetCardController(IImportService importService, IFleetCardRepository repository, ILoggingService loggingService)
        {
            _importService = importService;
            _repository = repository;
            _loggingService = loggingService;
        }

        /// <summary>
        /// นำเข้าไฟล์ข้อมูล Transaction (CSV หรือ Excel Report)
        /// </summary>
        /// <param name="file">ไฟล์ที่ต้องการนำเข้า</param>
        /// <returns>ผลลัพธ์การประมวลผลแต่ละแถว</returns>
        [HttpPost("import-transactions")]
        [ProducesResponseType(typeof(ImportResult<Dictionary<string, string?>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(object), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(typeof(object), StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> ImportTransactions([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "File not provided." });
            }

            try
            {
                var result = await _importService.ImportTransactionsAsync(file);
                return Ok(result);
            }
            catch (Exception ex)
            {
                // บันทึก Log สำหรับ Error ที่ไม่คาดคิดในระดับบนสุด
                await _loggingService.LogErrorAsync(ex, "An unhandled error occurred in the import process.", $"File: {file.FileName}");
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// ค้นหารายการ Transactions ตามเงื่อนไขต่างๆ
        /// </summary>
        [HttpGet("transactions/search")]
        [ProducesResponseType(typeof(IEnumerable<FleetCardTransaction>), StatusCodes.Status200OK)]
        public async Task<IActionResult> SearchTransactions([FromQuery] TransactionSearchCriteria criteria)
        {
            var transactions = await _repository.SearchTransactionsAsync(criteria);
            return Ok(transactions);
        }

        /// <summary>
        /// ดึงข้อมูลบัตร Master ทั้งหมดแบบแบ่งหน้า
        /// </summary>
        /// <param name="pageNumber">หมายเลขหน้า (เริ่มต้นที่ 1)</param>
        /// <param name="pageSize">จำนวนรายการต่อหน้า</param>
        [HttpGet("master")]
        [ProducesResponseType(typeof(IEnumerable<FleetCard>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllMasterData([FromQuery] int pageNumber = 1, [FromQuery] int pageSize = 20)
        {
            var cards = await _repository.GetAllFleetCardsAsync(pageNumber, pageSize);
            return Ok(cards);
        }

        /// <summary>
        /// ดึงข้อมูลบัตร Master หนึ่งใบตามหมายเลขบัตร
        /// </summary>
        /// <param name="cardNumber">หมายเลขบัตรที่ต้องการค้นหา</param>
        [HttpGet("master/{cardNumber}")]
        [ProducesResponseType(typeof(FleetCard), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetMasterByCardNumber(string cardNumber)
        {
            var card = await _repository.GetFleetCardByNumberAsync(cardNumber);
            if (card == null)
            {
                return NotFound();
            }
            return Ok(card);
        }

        /// <summary>
        /// สร้างหรืออัปเดตข้อมูลบัตร Master
        /// </summary>
        [HttpPost("master")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> UpsertMasterCard([FromBody] FleetCard card)
        {
            await _repository.UpsertFleetCardAsync(card);
            return NoContent();
        }
    }
}