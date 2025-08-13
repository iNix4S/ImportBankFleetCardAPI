using ImportBankFleetCardAPI.Config;

namespace ImportBankFleetCardAPI.Config
{
    /// <summary>
    /// Interface สำหรับ Service ที่ทำหน้าที่ดึงข้อมูลการตั้งค่า Template
    /// </summary>
    public interface IConfigService
    {
        /// <summary>
        /// ดึงข้อมูลการตั้งค่าของ Template ตามชื่อที่ระบุ
        /// </summary>
        /// <param name="templateName">ชื่อของ Template ที่ต้องการ</param>
        /// <returns>Dictionary ที่มี Key เป็น FieldName และ Value เป็น Object ของ Config</returns>
        Task<Dictionary<string, TemplateFieldConfig>> GetTemplateConfigAsync(string templateName);
    }
}