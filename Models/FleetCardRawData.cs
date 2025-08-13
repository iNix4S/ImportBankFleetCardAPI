using CsvHelper.Configuration.Attributes;

namespace ImportBankFleetCardAPI.Models
{
    public class FleetCardRawData
    {
        [Name("วันที่")] public string? TransactionDateStr { get; set; }
        [Name("เวลา")] public string? TransactionTimeStr { get; set; }
        [Name("หมายเลขบัตร")] public string? CardNumber { get; set; }
        [Name("ทะเบียนรถ")] [Optional] public string? PlateNumber { get; set; }
        [Name("ประเภทบัตร")] [Optional] public string? CardType { get; set; }
        [Name("ชื่อผู้ถือบัตร")] [Optional] public string? CardHolderName { get; set; }
        [Name("สถานีบริการ")] [Optional] public string? StationName { get; set; }
        [Name("ผลิตภัณฑ์")] [Optional] public string? ProductName { get; set; }
        [Name("จำนวน")] public string? Quantity { get; set; }
        [Name("ราคา/หน่วย")] [Optional] public string? UnitPrice { get; set; }
        [Name("ยอดรวม")] public string? TotalAmount { get; set; }
        [Name("เลขไมล์")] [Optional] public string? Odometer { get; set; }
        [Name("เลขที่ใบกำกับ")] [Optional] public string? InvoiceNo { get; set; }
    }
}