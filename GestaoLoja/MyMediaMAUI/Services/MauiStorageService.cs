using RCLAPI.Services;

namespace MyMediaMAUI.Services
{
    /// <summary>
    /// Implementação de armazenamento local usando Preferences do MAUI
    /// </summary>
    public class MauiStorageService : ILocalStorageService
    {
        public Task<string?> GetItemAsync(string key)
        {
            try
            {
                var value = Preferences.Get(key, null as string);
                return Task.FromResult(value);
            }
            catch
            {
                return Task.FromResult<string?>(null);
            }
        }

        public Task SetItemAsync(string key, string value)
        {
            try
            {
                Preferences.Set(key, value);
            }
            catch { }
            return Task.CompletedTask;
        }

        public Task RemoveItemAsync(string key)
        {
            try
            {
                Preferences.Remove(key);
            }
            catch { }
            return Task.CompletedTask;
        }
    }
}
