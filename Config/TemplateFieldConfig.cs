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
        public required string FieldName { get; init; }

        /// <summary>
        /// ชื่อ Header ในไฟล์ CSV ที่ตรงกับฟิลด์นี้
        /// </summary>
        public string? SourceColumnName { get; init; }

        /// <summary>
        /// ลำดับคอลัมน์ในไฟล์ Excel ที่ตรงกับฟิลด์นี้
        /// </summary>
        public int? SourceColumnIndex { get; init; }

        /// <summary>
        /// ฟิลด์นี้จำเป็นต้องมีข้อมูลหรือไม่
        /// </summary>
        public required bool IsRequired { get; init; }
    }
}