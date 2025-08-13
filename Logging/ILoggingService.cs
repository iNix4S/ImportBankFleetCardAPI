namespace ImportBankFleetCardAPI.Logging
{
    /// <summary>
    /// Interface สำหรับ Service ที่ทำหน้าที่บันทึก Log
    /// </summary>
    public interface ILoggingService
    {
        /// <summary>
        /// บันทึกข้อผิดพลาด (Exception) ลงในแหล่งจัดเก็บ
        /// </summary>
        /// <param name="ex">Exception ที่เกิดขึ้น</param>
        /// <param name="message">ข้อความสรุปเพิ่มเติม</param>
        /// <param name="contextInfo">ข้อมูลแวดล้อมที่เกี่ยวข้อง</param>
        Task LogErrorAsync(Exception ex, string message, string? contextInfo = null);
    }
}