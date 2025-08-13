using ImportBankFleetCardAPI.Config;
using System.Collections.Generic;

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
        /// <returns>Read-only Dictionary ที่มี Key เป็น FieldName และ Value เป็น Object ของ Config</returns>
        Task<IReadOnlyDictionary<string, TemplateFieldConfig>> GetTemplateConfigAsync(string templateName);

        /// <summary>
        /// ล้างข้อมูล Cache ของ Template ที่ระบุ เพื่อให้มีการดึงข้อมูลใหม่จากฐานข้อมูลในครั้งถัดไป
        /// </summary>
        /// <param name="templateName">ชื่อของ Template ที่ต้องการล้าง Cache</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        Task ClearCacheForTemplateAsync(string templateName);
    }
}