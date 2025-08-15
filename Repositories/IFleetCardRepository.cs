using ImportBankFleetCardAPI.DTOs;
using ImportBankFleetCardAPI.Models;

namespace ImportBankFleetCardAPI.Repositories
{
    /// <summary>
    /// Interface สำหรับ Repository ที่จัดการการเชื่อมต่อฐานข้อมูล Fleet Card
    /// </summary>
        public interface IFleetCardRepository
        {
            /// <summary>
            /// Insert transaction record (for test/demo)
            /// </summary>
            Task InsertTransactionAsync(FleetCardTransaction transaction);
        /// <summary>
        /// สร้างหรืออัปเดตข้อมูลบัตร Master
        /// </summary>
        Task UpsertFleetCardAsync(FleetCard card);

        /// <summary>
        /// ส่งข้อมูลดิบ 1 แถวไปให้ Stored Procedure ประมวลผล
        /// </summary>
        /// <returns>Tuple ที่มีสถานะ (Status) และข้อความผิดพลาด (ErrorMessage)</returns>
        Task<(string Status, string ErrorMessage)> ProcessTransactionRowAsync(Dictionary<string, string?> rowData);

        /// <summary>
        /// ดึงข้อมูลบัตร Master หนึ่งใบตามหมายเลขบัตร
        /// </summary>
        Task<FleetCard?> GetFleetCardByNumberAsync(string cardNumber);

        /// <summary>
        /// ดึงข้อมูลบัตร Master ทั้งหมดแบบแบ่งหน้า
        /// </summary>
        Task<IEnumerable<FleetCard>> GetAllFleetCardsAsync(int pageNumber, int pageSize);

        /// <summary>
        /// ค้นหารายการ Transactions ตามเงื่อนไขต่างๆ
        /// </summary>
        Task<IEnumerable<FleetCardTransaction>> SearchTransactionsAsync(TransactionSearchCriteria criteria);

        /// <summary>
        /// ดึงข้อมูลบัตร Master หนึ่งใบตามรหัสประจำตัว
        /// </summary>
        Task<FleetCard?> GetFleetCardByIdAsync(int id);
    }
}