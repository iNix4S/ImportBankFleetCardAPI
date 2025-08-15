namespace ImportBankFleetCardAPI.Models
{
    public class FleetCard
    {
        public int Id { get; set; }
        public string? CardNumber { get; set; }
        public string? DriverName { get; set; }
        public string? VehiclePlate { get; set; }
        public DateTime ExpiryDate { get; set; }
        public string? Status { get; set; }
        public string? CardType { get; set; }
        public string? CardHolderName { get; set; }
        public string? Name { get; set; }
    }
}