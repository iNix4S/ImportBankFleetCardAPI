namespace ImportBankFleetCardAPI.DTOs
{
    /// <summary>
    /// คลาสสำหรับเก็บรายละเอียดของแถวที่ Import ไม่สำเร็จ
    /// </summary>
    public class ImportFailureDetail
    {
        /// <summary>
        /// หมายเลขแถวในไฟล์ที่เกิดข้อผิดพลาด
        /// </summary>
        public int RowNumber { get; set; }

        /// <summary>
        /// ข้อมูลดิบจากแถวนั้นๆ
        /// </summary>
        public string? RawData { get; set; }

        /// <summary>
        /// ข้อความอธิบายข้อผิดพลาด
        /// </summary>
        public string ErrorMessage { get; set; } = string.Empty;
    }
}