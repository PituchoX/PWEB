using MyMediaWeb.Components;
using RCLAPI.Services;

var builder = WebApplication.CreateBuilder(args);

// URL da API RESTful
var apiBaseUrl = builder.Configuration["ApiBaseUrl"] ?? "https://43tc4dk1-7104.uks1.devtunnels.ms/";

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configurar HttpClient para comunicação com a API
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri(apiBaseUrl)
});

// Registar ApiService com ApiBaseUrl configurado
builder.Services.AddScoped(sp =>
{
    var httpClient = sp.GetRequiredService<HttpClient>();
    var apiService = new ApiService(httpClient);
    apiService.ApiBaseUrl = apiBaseUrl;
    return apiService;
});

// Registar CarrinhoService
builder.Services.AddScoped<CarrinhoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddAdditionalAssemblies(
        typeof(RCLComum._Imports).Assembly,
        typeof(RCLProdutos._Imports).Assembly,
        typeof(RCLCompras._Imports).Assembly
    );

app.Run();
