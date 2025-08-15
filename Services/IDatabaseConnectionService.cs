namespace ImportBankFleetCardAPI.Services
{
    public interface IDatabaseConnectionService
    {
        Task<bool> TestConnection();
        System.Threading.Tasks.Task<Oracle.ManagedDataAccess.Client.OracleConnection> GetOpenConnectionAsync();
    }
}
