using Microsoft.Extensions.Logging;
using RCLAPI.Services;

namespace MyMediaMAUI
{
    public static class MauiProgram
    {
        // ============================================================
        // URL da API RESTful
        // ============================================================
        // O Dev Tunnel ja esta configurado!
        // Certifica-te que a API (RESTfulAPIPWeb) esta a correr
        // antes de iniciar o MAUI.
        // ============================================================
        
        // URL do Dev Tunnel (ja configurado)
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

            // Configurar HttpClient para comunicacao com a API
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

            // Registar servicos
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
