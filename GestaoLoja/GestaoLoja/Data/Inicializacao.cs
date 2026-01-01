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
            else
            {
                // Garantir que o admin tem a role
                var adminRoles = userManager.GetRolesAsync(admin).Result;
                if (!adminRoles.Contains("Administrador"))
                {
                    userManager.AddToRoleAsync(admin, "Administrador").Wait();
                }
            }

            // ========================================
            // CRIAR/RESTAURAR FUNCIONÁRIO (para testes)
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
            else
            {
                // Restaurar o funcionário se foi removido/alterado
                bool alterado = false;
                
                if (funcionario.Estado != "Ativo")
                {
                    funcionario.Estado = "Ativo";
                    alterado = true;
                }
                
                if (funcionario.Perfil != "Funcionário")
                {
                    funcionario.Perfil = "Funcionário";
                    alterado = true;
                }
                
                if (alterado)
                {
                    userManager.UpdateAsync(funcionario).Wait();
                }

                // Garantir que tem a role
                var funcRoles = userManager.GetRolesAsync(funcionario).Result;
                if (!funcRoles.Contains("Funcionário"))
                {
                    userManager.AddToRoleAsync(funcionario, "Funcionário").Wait();
                }
            }

            // ========================================
            // CRIAR CLIENTE DE TESTE
            // ========================================
            const string clienteEmail = "cliente@teste.pt";
            const string clientePass = "Cliente123!";

            var clienteUser = userManager.FindByEmailAsync(clienteEmail).Result;
            if (clienteUser == null)
            {
                clienteUser = new ApplicationUser
                {
                    UserName = clienteEmail,
                    Email = clienteEmail,
                    NomeCompleto = "João Silva (Cliente Teste)",
                    Estado = "Ativo",
                    Perfil = "Cliente",
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(clienteUser, clientePass).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(clienteUser, "Cliente").Wait();
                }
            }
            else
            {
                // Garantir que está ativo
                if (clienteUser.Estado != "Ativo")
                {
                    clienteUser.Estado = "Ativo";
                    userManager.UpdateAsync(clienteUser).Wait();
                }

                // Garantir que tem a role
                var clienteRoles = userManager.GetRolesAsync(clienteUser).Result;
                if (!clienteRoles.Contains("Cliente"))
                {
                    userManager.AddToRoleAsync(clienteUser, "Cliente").Wait();
                }
            }

            // Criar registo na tabela Clientes
            var clienteTeste = context.Clientes.FirstOrDefault(c => c.ApplicationUserId == clienteUser.Id);
            if (clienteTeste == null)
            {
                clienteTeste = new Cliente
                {
                    ApplicationUserId = clienteUser.Id,
                    NIF = "123456789",
                    Estado = "Ativo"
                };
                context.Clientes.Add(clienteTeste);
                context.SaveChanges();
            }

            // ========================================
            // CRIAR FORNECEDOR DE TESTE
            // ========================================
            const string fornecedorEmail = "fornecedor@teste.pt";
            const string fornecedorPass = "Forn123!";

            var fornecedorUser = userManager.FindByEmailAsync(fornecedorEmail).Result;
            if (fornecedorUser == null)
            {
                fornecedorUser = new ApplicationUser
                {
                    UserName = fornecedorEmail,
                    Email = fornecedorEmail,
                    NomeCompleto = "Maria Santos (Fornecedor Teste)",
                    Estado = "Ativo",
                    Perfil = "Fornecedor",
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(fornecedorUser, fornecedorPass).Result;

                if (result.Succeeded)
                {
                    userManager.AddToRoleAsync(fornecedorUser, "Fornecedor").Wait();
                }
            }
            else
            {
                // Garantir que está ativo
                if (fornecedorUser.Estado != "Ativo")
                {
                    fornecedorUser.Estado = "Ativo";
                    userManager.UpdateAsync(fornecedorUser).Wait();
                }

                // Garantir que tem a role
                var fornRoles = userManager.GetRolesAsync(fornecedorUser).Result;
                if (!fornRoles.Contains("Fornecedor"))
                {
                    userManager.AddToRoleAsync(fornecedorUser, "Fornecedor").Wait();
                }
            }

            // Criar registo na tabela Fornecedores
            var fornecedorTeste = context.Fornecedores.FirstOrDefault(f => f.ApplicationUserId == fornecedorUser.Id);
            if (fornecedorTeste == null)
            {
                fornecedorTeste = new Fornecedor
                {
                    ApplicationUserId = fornecedorUser.Id,
                    NomeEmpresa = "MediaTeste Lda.",
                    Estado = "Aprovado"
                };
                context.Fornecedores.Add(fornecedorTeste);
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
                var catColeccionaveis = context.Categorias.First(c => c.Nome == "Colecionáveis");
                var modoEntrega = context.ModosEntrega.First();

                // Usar o fornecedor de teste
                var fornTeste = context.Fornecedores.First(f => f.NomeEmpresa == "MediaTeste Lda.");

                context.Produtos.AddRange(
                    // FILMES
                    new Produtos
                    {
                        Nome = "Avatar - DVD",
                        PrecoBase = 14.29m,
                        Percentagem = 10,
                        PrecoFinal = 15.72m,
                        Estado = "Ativo",
                        Stock = 50,
                        Imagem = "noproductstrans.png",
                        CategoriaId = catFilmes.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
                    },
                    new Produtos
                    {
                        Nome = "O Senhor dos Anéis - Blu-ray",
                        PrecoBase = 22.99m,
                        Percentagem = 15,
                        PrecoFinal = 26.44m,
                        Estado = "Ativo",
                        Stock = 30,
                        Imagem = "noproductstrans.png",
                        CategoriaId = catFilmes.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
                    },
                    new Produtos
                    {
                        Nome = "Matrix - Edição Especial",
                        PrecoBase = 27.49m,
                        Percentagem = 10,
                        PrecoFinal = 30.24m,
                        Estado = "Ativo",
                        Stock = 25,
                        Imagem = "noproductstrans.png",
                        CategoriaId = catFilmes.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
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
                        Imagem = "noproductstrans.png",
                        CategoriaId = catMusica.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
                    },
                    new Produtos
                    {
                        Nome = "Pink Floyd - The Wall - Vinil",
                        PrecoBase = 34.99m,
                        Percentagem = 10,
                        PrecoFinal = 38.49m,
                        Estado = "Ativo",
                        Stock = 15,
                        Imagem = "noproductstrans.png",
                        CategoriaId = catMusica.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
                    },
                    new Produtos
                    {
                        Nome = "Beatles - Abbey Road - CD",
                        PrecoBase = 12.99m,
                        Percentagem = 5,
                        PrecoFinal = 13.64m,
                        Estado = "Ativo",
                        Stock = 35,
                        Imagem = "noproductstrans.png",
                        CategoriaId = catMusica.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
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
                        Imagem = "noproductstrans.png",
                        CategoriaId = catJogos.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
                    },
                    new Produtos
                    {
                        Nome = "GTA V - Xbox",
                        PrecoBase = 29.99m,
                        Percentagem = 20,
                        PrecoFinal = 35.99m,
                        Estado = "Ativo",
                        Stock = 45,
                        Imagem = "noproductstrans.png",
                        CategoriaId = catJogos.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
                    },
                    new Produtos
                    {
                        Nome = "Minecraft - PC",
                        PrecoBase = 19.99m,
                        Percentagem = 0,
                        PrecoFinal = 19.99m,
                        Estado = "Ativo",
                        Stock = 200,
                        Imagem = "noproductstrans.png",
                        CategoriaId = catJogos.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
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
                        Imagem = "noproductstrans.png",
                        CategoriaId = catAcessorios.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
                    },
                    new Produtos
                    {
                        Nome = "Headset Gaming RGB",
                        PrecoBase = 49.99m,
                        Percentagem = 10,
                        PrecoFinal = 54.99m,
                        Estado = "Ativo",
                        Stock = 80,
                        Imagem = "noproductstrans.png",
                        CategoriaId = catAcessorios.Id,
                        ModoEntregaId = modoEntrega.Id,
                        FornecedorId = fornTeste.Id
                    }
                );
                context.SaveChanges();
            }
        }
    }
}
