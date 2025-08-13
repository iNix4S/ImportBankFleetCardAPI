namespace ImportBankFleetCardAPI.Services
{
    public interface IDatabaseConnectionService
    {
        Task<bool> TestConnection();
    }
}
