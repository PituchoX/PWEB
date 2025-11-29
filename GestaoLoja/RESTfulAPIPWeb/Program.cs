using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Data;
using RESTfulAPI.Repositories.Services;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories.Interfaces;
using RESTfulAPIPWeb.Repositories.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1. Configurar Base de Dados (AppDbContext)
// ----------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ----------------------------------------------------
// 2. Configurar Identity (necessário por causa do ApplicationUser)
// ----------------------------------------------------
builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<AppDbContext>();

// ----------------------------------------------------
// 3. Adicionar Controladores
// ----------------------------------------------------
builder.Services.AddControllers();

// ----------------------------------------------------
// 4. Swagger (documentação da API)
// ----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------------------------------------------
// 5. Injeção de Dependências dos Repositórios
// ----------------------------------------------------
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IModoEntregaRepository, ModoEntregaRepository>();

// ----------------------------------------------------
// 6. Ativar CORS (para permitir acesso externo, como Blazor, DevTunnel, etc.)
// ----------------------------------------------------
builder.Services.AddCors(options =>
{
    options.AddPolicy("allowAll",
        p => p.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod()
        );
});

var app = builder.Build();

// ----------------------------------------------------
// 7. Swagger no modo Development
// ----------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ----------------------------------------------------
// 8. Middleware HTTP
// ----------------------------------------------------
app.UseHttpsRedirection();

app.UseCors("allowAll");

app.UseAuthentication();  // ← obrigatório agora
app.UseAuthorization();

app.MapControllers();

app.Run();
