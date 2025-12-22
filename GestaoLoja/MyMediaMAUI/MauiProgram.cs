using Microsoft.Extensions.Logging;
using RCLAPI.Services;

namespace MyMediaMAUI
{
    public static class MauiProgram
    {
        // ============================================================
        // URL da API RESTful
        // ============================================================
        // Em DESENVOLVIMENTO: Usar Dev Tunnel URL para Android/iOS
        // Para Windows Machine pode usar localhost
        // 
        // Para obter o Dev Tunnel:
        // 1. No Visual Studio, clica na seta ao lado do botão Debug
        // 2. Seleciona "Dev Tunnels" → "Create a Tunnel..."
        // 3. Copia a URL gerada e cola aqui
        // ============================================================
        
#if DEBUG
        // Para WINDOWS pode usar localhost (porta 7104)
        // Para ANDROID/iOS precisa de Dev Tunnel URL
        public const string ApiBaseUrl = "https://localhost:7104/";
#else
        // URL de produção (quando publicares a API)
        public const string ApiBaseUrl = "https://mymedia-api.azurewebsites.net/";
#endif

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
            // Ignorar erros de certificado SSL em desenvolvimento
            builder.Services.AddScoped(sp =>
            {
#if DEBUG
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
                };
                return new HttpClient(handler)
                {
                    BaseAddress = new Uri(ApiBaseUrl)
                };
#else
                return new HttpClient
                {
                    BaseAddress = new Uri(ApiBaseUrl)
                };
#endif
            });

            // Registar serviços
            builder.Services.AddScoped<ApiService>();
            builder.Services.AddSingleton<CarrinhoService>();

#if DEBUG
            builder.Services.AddBlazorWebViewDeveloperTools();
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
