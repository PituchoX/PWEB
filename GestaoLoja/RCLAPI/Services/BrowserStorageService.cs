using Microsoft.JSInterop;

namespace RCLAPI.Services
{
    /// <summary>
    /// Implementação de armazenamento local usando localStorage do browser
    /// Para uso em Blazor WebAssembly/Server
    /// </summary>
    public class BrowserStorageService : ILocalStorageService
    {
        private readonly IJSRuntime _jsRuntime;

        public BrowserStorageService(IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task<string?> GetItemAsync(string key)
        {
            try
            {
                return await _jsRuntime.InvokeAsync<string?>("localStorage.getItem", key);
            }
            catch
            {
                return null;
            }
        }

        public async Task SetItemAsync(string key, string value)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", key, value);
            }
            catch
            {
                // Ignora erros (ex: prerendering)
            }
        }

        public async Task RemoveItemAsync(string key)
        {
            try
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", key);
            }
            catch
            {
                // Ignora erros
            }
        }
    }
}
