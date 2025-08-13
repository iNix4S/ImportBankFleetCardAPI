namespace ImportBankFleetCardAPI.Config
{
    /// <summary>
    /// คลาสสำหรับเก็บข้อมูลการตั้งค่าของแต่ละฟิลด์ใน Template
    /// </summary>
    public class TemplateFieldConfig
    {
        /// <summary>
        /// ชื่อฟิลด์ข้อมูลทางตรรกะ (เช่น 'CardNumber', 'TransactionDate')
        /// </summary>
        public string FieldName { get; set; } = string.Empty;

        /// <summary>
        /// ชื่อ Header ในไฟล์ CSV ที่ตรงกับฟิลด์นี้
        /// </summary>
        public string? SourceColumnName { get; set; }

        /// <summary>
        /// ลำดับคอลัมน์ในไฟล์ Excel ที่ตรงกับฟิลด์นี้
        /// </summary>
        public int? SourceColumnIndex { get; set; }

        /// <summary>
        /// ฟิลด์นี้จำเป็นต้องมีข้อมูลหรือไม่
        /// </summary>
        public bool IsRequired { get; set; }
    }
}