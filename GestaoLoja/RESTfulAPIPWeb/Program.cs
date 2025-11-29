using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RESTfulAPIPWeb.Data;
using RESTfulAPI.Repositories.Services;
using RESTfulAPIPWeb.Entities;
using RESTfulAPIPWeb.Repositories.Interfaces;
using RESTfulAPIPWeb.Repositories.Services;
using System.Text;



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


var jwtSettings = builder.Configuration.GetSection("Jwt");
var key = Encoding.UTF8.GetBytes(jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtSettings["Audience"],
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(key),
        ValidateLifetime = true
    };
});

builder.Services.AddAuthorization();

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

app.UseAuthentication();
app.UseAuthorization();


app.UseCors("allowAll");

app.UseAuthentication();  // ← obrigatório agora
app.UseAuthorization();

app.MapControllers();

app.Run();
