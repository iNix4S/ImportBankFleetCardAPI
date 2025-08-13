namespace ImportBankFleetCardAPI.DTOs
{
    /// <summary>
    /// คลาสสำหรับสรุปผลลัพธ์ของกระบวนการ Import
    /// </summary>
    /// <typeparam name="T">ประเภทของข้อมูลที่ประมวลผลสำเร็จ</typeparam>
    public class ImportResult<T>
    {
        /// <summary>
        /// จำนวนแถวข้อมูลทั้งหมดในไฟล์
        /// </summary>
        public int TotalRowsInFile { get; set; }

        /// <summary>
        /// จำนวนแถวที่ประมวลผลสำเร็จ
        /// </summary>
        public int SuccessfulRows { get; set; }

        /// <summary>
        /// จำนวนแถวที่ประมวลผลล้มเหลว
        /// </summary>
        public int FailedRows { get; set; }

        /// <summary>
        /// รายละเอียดของข้อมูลที่สำเร็จ
        /// </summary>
        public List<T> SuccessDetails { get; set; } = new List<T>();

        /// <summary>
        /// รายละเอียดของข้อมูลที่ล้มเหลว
        /// </summary>
        public List<ImportFailureDetail> FailureDetails { get; set; } = new List<ImportFailureDetail>();
    }
}