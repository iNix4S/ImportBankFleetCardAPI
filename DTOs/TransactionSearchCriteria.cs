namespace ImportBankFleetCardAPI.DTOs
{
    /// <summary>
    /// คลาสสำหรับรับเงื่อนไขในการค้นหารายการ Transactions
    /// </summary>
    public class TransactionSearchCriteria
    {
        public string? CardNumber { get; set; }
        public string? PlateNumber { get; set; }
        public string? InvoiceNo { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}