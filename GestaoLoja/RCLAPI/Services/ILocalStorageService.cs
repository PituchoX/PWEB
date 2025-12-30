namespace RCLAPI.Services
{
    /// <summary>
    /// Interface para armazenamento local de dados da sessão
    /// Implementações diferentes para Web (localStorage) e MAUI (Preferences)
    /// </summary>
    public interface ILocalStorageService
    {
        Task<string?> GetItemAsync(string key);
        Task SetItemAsync(string key, string value);
        Task RemoveItemAsync(string key);
    }
}
