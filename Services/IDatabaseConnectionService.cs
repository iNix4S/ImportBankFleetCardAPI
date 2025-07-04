namespace SecureFileAPI.Services
{
    public interface IDatabaseConnectionService
    {
        Task<bool> TestConnection();
    }
}
