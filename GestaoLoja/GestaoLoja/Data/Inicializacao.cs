using GestaoLoja.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace GestaoLoja.Data
{
    public static class Inicializacao
    {
        public static void SeedDatabase(IServiceProvider services)
        {
            using var scope = services.CreateScope();

            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            // Aplica migrações pendentes
            context.Database.Migrate();

            // ========================================
            // CRIAR TODOS OS ROLES
            // ========================================
            string[] roles = { "Administrador", "Funcionário", "Cliente", "Fornecedor" };

            foreach (var role in roles)
            {
                if (!roleManager.RoleExistsAsync(role).Result)
                {
                    roleManager.CreateAsync(new IdentityRole(role)).Wait();
                }
            }

            // ========================================
            // CRIAR ADMINISTRADOR
            // ========================================
            const string adminEmail = "admin@gestao.pt";
            const string adminPass = "Admin123!";

            var admin = userManager.FindByEmailAsync(adminEmail).Result;
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Administrador do Sistema",
                    Estado = "Ativo",
                    Perfil = "Administrador",
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(admin, adminPass).Result;

                if (!result.Succeeded)
                {
                    throw new Exception("Erro a criar admin: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                userManager.AddToRoleAsync(admin, "Administrador").Wait();
            }

            // ========================================
            // CRIAR FUNCIONÁRIO (para testes)
            // ========================================
            const string funcEmail = "func@gestao.pt";
            const string funcPass = "Func123!";

            var funcionario = userManager.FindByEmailAsync(funcEmail).Result;
            if (funcionario == null)
            {
                funcionario = new ApplicationUser
                {
                    UserName = funcEmail,
                    Email = funcEmail,
                    NomeCompleto = "Funcionário Exemplo",
                    Estado = "Ativo",
                    Perfil = "Funcionário",
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(funcionario, funcPass).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(funcionario, "Funcionário").Wait();
                }
            }

            // ========================================
            // CRIAR FORNECEDOR INTERNO
            // ========================================
            var fornecedorInterno = context.Fornecedores
                .FirstOrDefault(f => f.NomeEmpresa == "Fornecedor Interno");

            if (fornecedorInterno == null)
            {
                fornecedorInterno = new Fornecedor
                {
                    NomeEmpresa = "Fornecedor Interno",
                    NIF = "000000000",
                    Estado = "Aprovado",
                    ApplicationUserId = admin.Id
                };

                context.Fornecedores.Add(fornecedorInterno);
                context.SaveChanges();
            }

            // ========================================
            // MODOS DE ENTREGA INICIAIS
            // ========================================
            if (!context.ModosEntrega.Any())
            {
                context.ModosEntrega.AddRange(
                    new ModoEntrega
                    {
                        Nome = "Levantamento em loja",
                        Tipo = "Levantamento",
                        Detalhe = "Levantar na loja física"
                    },
                    new ModoEntrega
                    {
                        Nome = "Entrega ao domicílio",
                        Tipo = "Entrega",
                        Detalhe = "Entrega em morada indicada"
                    },
                    new ModoEntrega
                    {
                        Nome = "Ponto de recolha",
                        Tipo = "Recolha",
                        Detalhe = "Ponto de recolha parceiro"
                    }
                );
                context.SaveChanges();
            }

            // ========================================
            // CATEGORIAS INICIAIS (exemplo MyMEDIA)
            // ========================================
            if (!context.Categorias.Any())
            {
                context.Categorias.AddRange(
                    new Categorias { Nome = "Filmes", Imagem = "filmes.png" },
                    new Categorias { Nome = "Música", Imagem = "musica.png" },
                    new Categorias { Nome = "Jogos", Imagem = "jogos.png" },
                    new Categorias { Nome = "Acessórios", Imagem = "acessorios.png" },
                    new Categorias { Nome = "Colecionáveis", Imagem = "coleccionaveis.png" }
                );
                context.SaveChanges();
            }

            // ========================================
            // PRODUTOS INICIAIS (exemplo MyMEDIA)
            // ========================================
            if (!context.Produtos.Any())
            {
                // Obter IDs das categorias e modo de entrega
                var catFilmes = context.Categorias.First(c => c.Nome == "Filmes");
                var catMusica = context.Categorias.First(c => c.Nome == "Música");
                var catJogos = context.Categorias.First(c => c.Nome == "Jogos");
                var catAcessorios = context.Categorias.First(c => c.Nome == "Acessórios");
                var modoEntrega = context.ModosEntrega.First();

                context.Produtos.AddRange(
                    // FILMES
                    new Produtos
                    {
                        Nome = "Avatar - DVD",
                        PrecoBase = 12.99m,
                        Percentagem = 10,
                        PrecoFinal = 14.29m,
                        Estado = "Ativo",
                        Stock = 50,
                        Imagem = "avatar.png",
                        CategoriaId = catFilmes.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    new Produtos
                    {
                        Nome = "O Senhor dos Anéis - Blu-ray",
                        PrecoBase = 19.99m,
                        Percentagem = 15,
                        PrecoFinal = 22.99m,
                        Estado = "Ativo",
                        Stock = 30,
                        Imagem = "senhor_aneis.png",
                        CategoriaId = catFilmes.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    new Produtos
                    {
                        Nome = "Matrix - Edição Especial",
                        PrecoBase = 24.99m,
                        Percentagem = 10,
                        PrecoFinal = 27.49m,
                        Estado = "Ativo",
                        Stock = 25,
                        Imagem = "matrix.png",
                        CategoriaId = catFilmes.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    
                    // MÚSICA
                    new Produtos
                    {
                        Nome = "Queen - Greatest Hits - CD",
                        PrecoBase = 14.99m,
                        Percentagem = 5,
                        PrecoFinal = 15.74m,
                        Estado = "Ativo",
                        Stock = 40,
                        Imagem = "queen.png",
                        CategoriaId = catMusica.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    new Produtos
                    {
                        Nome = "Pink Floyd - The Wall - Vinil",
                        PrecoBase = 34.99m,
                        Percentagem = 10,
                        PrecoFinal = 38.49m,
                        Estado = "Ativo",
                        Stock = 15,
                        Imagem = "pinkfloyd.png",
                        CategoriaId = catMusica.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    new Produtos
                    {
                        Nome = "Beatles - Abbey Road - CD",
                        PrecoBase = 12.99m,
                        Percentagem = 5,
                        PrecoFinal = 13.64m,
                        Estado = "Ativo",
                        Stock = 35,
                        Imagem = "beatles.png",
                        CategoriaId = catMusica.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    
                    // JOGOS
                    new Produtos
                    {
                        Nome = "FIFA 24 - PS5",
                        PrecoBase = 59.99m,
                        Percentagem = 0,
                        PrecoFinal = 59.99m,
                        Estado = "Ativo",
                        Stock = 100,
                        Imagem = "fifa24.png",
                        CategoriaId = catJogos.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    new Produtos
                    {
                        Nome = "GTA V - Xbox",
                        PrecoBase = 29.99m,
                        Percentagem = 20,
                        PrecoFinal = 35.99m,
                        Estado = "Ativo",
                        Stock = 45,
                        Imagem = "gtav.png",
                        CategoriaId = catJogos.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    new Produtos
                    {
                        Nome = "Minecraft - PC",
                        PrecoBase = 19.99m,
                        Percentagem = 0,
                        PrecoFinal = 19.99m,
                        Estado = "Ativo",
                        Stock = 200,
                        Imagem = "minecraft.png",
                        CategoriaId = catJogos.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    
                    // ACESSÓRIOS
                    new Produtos
                    {
                        Nome = "Comando PS5 DualSense",
                        PrecoBase = 69.99m,
                        Percentagem = 5,
                        PrecoFinal = 73.49m,
                        Estado = "Ativo",
                        Stock = 60,
                        Imagem = "dualsense.png",
                        CategoriaId = catAcessorios.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    },
                    new Produtos
                    {
                        Nome = "Headset Gaming RGB",
                        PrecoBase = 49.99m,
                        Percentagem = 10,
                        PrecoFinal = 54.99m,
                        Estado = "Ativo",
                        Stock = 80,
                        Imagem = "headset.png",
                        CategoriaId = catAcessorios.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornecedorInterno.Id
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
