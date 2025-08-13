using ImportBankFleetCardAPI.DTOs;

namespace ImportBankFleetCardAPI.Services
{
    /// <summary>
    /// Interface สำหรับ Service หลักที่จัดการตรรกะการนำเข้าไฟล์
    /// </summary>
    public interface IImportService
    {
        /// <summary>
        /// ประมวลผลไฟล์ Transaction ที่อัปโหลดเข้ามา
        /// </summary>
        /// <param name="file">ไฟล์ที่ผู้ใช้อัปโหลด</param>
        /// <returns>ผลลัพธ์การประมวลผลโดยละเอียด</returns>
        Task<ImportResult<Dictionary<string, string?>>> ImportTransactionsAsync(IFormFile file);
    }
}