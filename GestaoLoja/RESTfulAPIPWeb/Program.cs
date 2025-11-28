using Microsoft.EntityFrameworkCore;
using RESTfulAPI.Data;
using RESTfulAPI.Repositories.Interfaces;
using RESTfulAPI.Repositories.Services;
using RESTfulAPI.Data;
using RESTfulAPI.Repositories.Interfaces;
using RESTfulAPI.Repositories.Services;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------
// 1. Configurar Base de Dados (AppDbContext)
// ----------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
);

// ----------------------------------------------------
// 2. Adicionar Controladores
// ----------------------------------------------------
builder.Services.AddControllers();

// ----------------------------------------------------
// 3. Swagger (documentação da API)
// ----------------------------------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ----------------------------------------------------
// 4. Injeção de Dependências dos Repositórios
// ----------------------------------------------------
builder.Services.AddScoped<ICategoriaRepository, CategoriaRepository>();
builder.Services.AddScoped<IProdutoRepository, ProdutoRepository>();
builder.Services.AddScoped<IModoEntregaRepository, ModoEntregaRepository>();

// ----------------------------------------------------
// 5. Ativar CORS (para permitir acesso externo, como Blazor, DevTunnel, etc.)
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
// 6. Swagger no modo Development
// ----------------------------------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ----------------------------------------------------
// 7. Middleware HTTP
// ----------------------------------------------------
app.UseHttpsRedirection();

app.UseCors("allowAll");

app.UseAuthorization();

app.MapControllers();

app.Run();
