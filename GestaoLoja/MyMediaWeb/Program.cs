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

// Registar serviços
builder.Services.AddScoped<ApiService>();
// --- MUDA DE AddSingleton PARA AddScoped ---
builder.Services.AddScoped<RCLAPI.Services.CarrinhoService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
