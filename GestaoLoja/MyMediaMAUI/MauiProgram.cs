using Microsoft.Extensions.Logging;
using RCLAPI.Services;
using MyMediaMAUI.Services;

namespace MyMediaMAUI
{
    public static class MauiProgram
    {
        // ============================================================
        // URL da API RESTful
        // ============================================================
        // O Dev Tunnel já está configurado!
        // Certifica-te que a API (RESTfulAPIPWeb) está a correr
        // antes de iniciar o MAUI.
        // ============================================================
        
        // URL do Dev Tunnel (já configurado)
        public const string ApiBaseUrl = "https://43tc4dk1-7104.uks1.devtunnels.ms/";

        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                });

            builder.Services.AddMauiBlazorWebView();

            // Configurar HttpClient para comunicação com a API
            builder.Services.AddScoped(sp =>
            {
                var handler = new HttpClientHandler
                {
                    // Ignorar erros de certificado SSL em desenvolvimento
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(ApiBaseUrl),
                    Timeout = TimeSpan.FromSeconds(30)
                };
            });

            // Registar serviço de armazenamento local (MAUI Preferences)
            builder.Services.AddSingleton<ILocalStorageService, MauiStorageService>();

            // Registar ApiService com ApiBaseUrl e Storage configurados
            builder.Services.AddScoped(sp =>
            {
                var httpClient = sp.GetRequiredService<HttpClient>();
                var storage = sp.GetRequiredService<ILocalStorageService>();
                var apiService = new ApiService(httpClient, storage);
                apiService.ApiBaseUrl = ApiBaseUrl;
                return apiService;
            });

            builder.Services.AddSingleton<CarrinhoService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
