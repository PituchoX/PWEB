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
                        Detalhe = "Levantar na loja física - Grátis"
                    },
                    new ModoEntrega
                    {
                        Nome = "Entrega ao domicílio",
                        Tipo = "Entrega",
                        Detalhe = "Entrega em 2-3 dias úteis"
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
            // SUBCATEGORIAS INICIAIS
            // ========================================
            if (!context.Subcategorias.Any())
            {
                var catFilmes = context.Categorias.First(c => c.Nome == "Filmes");
                var catMusica = context.Categorias.First(c => c.Nome == "Música");
                var catJogos = context.Categorias.First(c => c.Nome == "Jogos");
                var catAcessorios = context.Categorias.First(c => c.Nome == "Acessórios");
                var catColeccionaveis = context.Categorias.First(c => c.Nome == "Colecionáveis");

                context.Subcategorias.AddRange(
                    // Filmes (5 subcategorias)
                    new Subcategoria { Nome = "Ação", CategoriaId = catFilmes.Id },
                    new Subcategoria { Nome = "Comédia", CategoriaId = catFilmes.Id },
                    new Subcategoria { Nome = "Drama", CategoriaId = catFilmes.Id },
                    new Subcategoria { Nome = "Ficção Científica", CategoriaId = catFilmes.Id },
                    new Subcategoria { Nome = "Terror", CategoriaId = catFilmes.Id },
                    
                    // Música (5 subcategorias)
                    new Subcategoria { Nome = "Rock", CategoriaId = catMusica.Id },
                    new Subcategoria { Nome = "Pop", CategoriaId = catMusica.Id },
                    new Subcategoria { Nome = "Jazz", CategoriaId = catMusica.Id },
                    new Subcategoria { Nome = "Clássica", CategoriaId = catMusica.Id },
                    new Subcategoria { Nome = "Hip-Hop", CategoriaId = catMusica.Id },
                    
                    // Jogos (4 subcategorias)
                    new Subcategoria { Nome = "PlayStation", CategoriaId = catJogos.Id },
                    new Subcategoria { Nome = "Xbox", CategoriaId = catJogos.Id },
                    new Subcategoria { Nome = "Nintendo", CategoriaId = catJogos.Id },
                    new Subcategoria { Nome = "PC", CategoriaId = catJogos.Id },
                    
                    // Acessórios (3 subcategorias)
                    new Subcategoria { Nome = "Comandos", CategoriaId = catAcessorios.Id },
                    new Subcategoria { Nome = "Headsets", CategoriaId = catAcessorios.Id },
                    new Subcategoria { Nome = "Cabos", CategoriaId = catAcessorios.Id },
                    
                    // Colecionáveis (3 subcategorias)
                    new Subcategoria { Nome = "Figuras", CategoriaId = catColeccionaveis.Id },
                    new Subcategoria { Nome = "Posters", CategoriaId = catColeccionaveis.Id },
                    new Subcategoria { Nome = "Edições Limitadas", CategoriaId = catColeccionaveis.Id }
                );
                context.SaveChanges();
            }

            // ========================================
            // PRODUTOS INICIAIS (exemplo MyMEDIA)
            // ========================================
            if (!context.Produtos.Any())
            {
                var catFilmes = context.Categorias.First(c => c.Nome == "Filmes");
                var catMusica = context.Categorias.First(c => c.Nome == "Música");
                var catJogos = context.Categorias.First(c => c.Nome == "Jogos");
                var catAcessorios = context.Categorias.First(c => c.Nome == "Acessórios");
                var catColeccionaveis = context.Categorias.First(c => c.Nome == "Colecionáveis");

                // Subcategorias Filmes
                var subAcao = context.Subcategorias.First(s => s.Nome == "Ação");
                var subComedia = context.Subcategorias.First(s => s.Nome == "Comédia");
                var subDrama = context.Subcategorias.First(s => s.Nome == "Drama");
                var subFiccao = context.Subcategorias.First(s => s.Nome == "Ficção Científica");
                var subTerror = context.Subcategorias.First(s => s.Nome == "Terror");

                // Subcategorias Música
                var subRock = context.Subcategorias.First(s => s.Nome == "Rock");
                var subPop = context.Subcategorias.First(s => s.Nome == "Pop");
                var subJazz = context.Subcategorias.First(s => s.Nome == "Jazz");
                var subClassica = context.Subcategorias.First(s => s.Nome == "Clássica");
                var subHipHop = context.Subcategorias.First(s => s.Nome == "Hip-Hop");

                // Subcategorias Jogos
                var subPS = context.Subcategorias.First(s => s.Nome == "PlayStation");
                var subXbox = context.Subcategorias.First(s => s.Nome == "Xbox");
                var subNintendo = context.Subcategorias.First(s => s.Nome == "Nintendo");
                var subPC = context.Subcategorias.First(s => s.Nome == "PC");

                // Subcategorias Acessórios
                var subComandos = context.Subcategorias.First(s => s.Nome == "Comandos");
                var subHeadsets = context.Subcategorias.First(s => s.Nome == "Headsets");
                var subCabos = context.Subcategorias.First(s => s.Nome == "Cabos");

                // Subcategorias Colecionáveis
                var subFiguras = context.Subcategorias.First(s => s.Nome == "Figuras");
                var subPosters = context.Subcategorias.First(s => s.Nome == "Posters");
                var subEdicoes = context.Subcategorias.First(s => s.Nome == "Edições Limitadas");

                var modoEntrega = context.ModosEntrega.First();
                var fornTeste = context.Fornecedores.First(f => f.NomeEmpresa == "MediaTeste Lda.");

                var produtos = new List<Produtos>
                {
                    // ==================== FILMES - AÇÃO (3) ====================
                    CriarProduto("John Wick 4 - Blu-ray", 24.99m, 10, catFilmes.Id, subAcao.Id, modoEntrega.Id, fornTeste.Id, 30),
                    CriarProduto("Fast & Furious X - DVD", 19.99m, 5, catFilmes.Id, subAcao.Id, modoEntrega.Id, fornTeste.Id, 45),
                    CriarProduto("Top Gun: Maverick - 4K", 29.99m, 15, catFilmes.Id, subAcao.Id, modoEntrega.Id, fornTeste.Id, 25),

                    // ==================== FILMES - COMÉDIA (3) ====================
                    CriarProduto("Superbad - DVD", 12.99m, 5, catFilmes.Id, subComedia.Id, modoEntrega.Id, fornTeste.Id, 40),
                    CriarProduto("The Hangover Trilogy - Blu-ray", 29.99m, 10, catFilmes.Id, subComedia.Id, modoEntrega.Id, fornTeste.Id, 20),
                    CriarProduto("Borat - DVD", 9.99m, 0, catFilmes.Id, subComedia.Id, modoEntrega.Id, fornTeste.Id, 35),

                    // ==================== FILMES - DRAMA (3) ====================
                    CriarProduto("The Shawshank Redemption - Blu-ray", 14.99m, 10, catFilmes.Id, subDrama.Id, modoEntrega.Id, fornTeste.Id, 30),
                    CriarProduto("Forrest Gump - 4K", 24.99m, 5, catFilmes.Id, subDrama.Id, modoEntrega.Id, fornTeste.Id, 25),
                    CriarProduto("The Godfather Collection - Blu-ray", 49.99m, 15, catFilmes.Id, subDrama.Id, modoEntrega.Id, fornTeste.Id, 15),

                    // ==================== FILMES - FICÇÃO CIENTÍFICA (3) ====================
                    CriarProduto("Avatar - DVD", 14.29m, 10, catFilmes.Id, subFiccao.Id, modoEntrega.Id, fornTeste.Id, 50),
                    CriarProduto("Interstellar - 4K", 27.99m, 5, catFilmes.Id, subFiccao.Id, modoEntrega.Id, fornTeste.Id, 35),
                    CriarProduto("Matrix Collection - Blu-ray", 39.99m, 10, catFilmes.Id, subFiccao.Id, modoEntrega.Id, fornTeste.Id, 20),

                    // ==================== FILMES - TERROR (3) ====================
                    CriarProduto("The Conjuring - Blu-ray", 14.99m, 10, catFilmes.Id, subTerror.Id, modoEntrega.Id, fornTeste.Id, 40),
                    CriarProduto("IT Chapter 1 & 2 - DVD", 24.99m, 5, catFilmes.Id, subTerror.Id, modoEntrega.Id, fornTeste.Id, 30),
                    CriarProduto("A Nightmare on Elm Street - DVD", 12.99m, 0, catFilmes.Id, subTerror.Id, modoEntrega.Id, fornTeste.Id, 25),

                    // ==================== MÚSICA - ROCK (3) ====================
                    CriarProduto("Queen - Greatest Hits - CD", 14.99m, 5, catMusica.Id, subRock.Id, modoEntrega.Id, fornTeste.Id, 40),
                    CriarProduto("Pink Floyd - The Wall - Vinil", 34.99m, 10, catMusica.Id, subRock.Id, modoEntrega.Id, fornTeste.Id, 15),
                    CriarProduto("Led Zeppelin IV - Vinil", 32.99m, 10, catMusica.Id, subRock.Id, modoEntrega.Id, fornTeste.Id, 18),

                    // ==================== MÚSICA - POP (3) ====================
                    CriarProduto("Taylor Swift - 1989 - CD", 16.99m, 0, catMusica.Id, subPop.Id, modoEntrega.Id, fornTeste.Id, 60),
                    CriarProduto("The Weeknd - After Hours - Vinil", 27.99m, 5, catMusica.Id, subPop.Id, modoEntrega.Id, fornTeste.Id, 25),
                    CriarProduto("Dua Lipa - Future Nostalgia - CD", 14.99m, 5, catMusica.Id, subPop.Id, modoEntrega.Id, fornTeste.Id, 35),

                    // ==================== MÚSICA - JAZZ (3) ====================
                    CriarProduto("Miles Davis - Kind of Blue - CD", 12.99m, 10, catMusica.Id, subJazz.Id, modoEntrega.Id, fornTeste.Id, 30),
                    CriarProduto("John Coltrane - A Love Supreme - Vinil", 29.99m, 5, catMusica.Id, subJazz.Id, modoEntrega.Id, fornTeste.Id, 12),
                    CriarProduto("Dave Brubeck - Time Out - CD", 11.99m, 0, catMusica.Id, subJazz.Id, modoEntrega.Id, fornTeste.Id, 20),

                    // ==================== MÚSICA - CLÁSSICA (3) ====================
                    CriarProduto("Beethoven - 9 Sinfonias - Box CD", 49.99m, 15, catMusica.Id, subClassica.Id, modoEntrega.Id, fornTeste.Id, 10),
                    CriarProduto("Mozart - Requiem - CD", 14.99m, 5, catMusica.Id, subClassica.Id, modoEntrega.Id, fornTeste.Id, 20),
                    CriarProduto("Vivaldi - As Quatro Estações - Vinil", 24.99m, 10, catMusica.Id, subClassica.Id, modoEntrega.Id, fornTeste.Id, 15),

                    // ==================== MÚSICA - HIP-HOP (3) ====================
                    CriarProduto("Kendrick Lamar - DAMN - CD", 14.99m, 5, catMusica.Id, subHipHop.Id, modoEntrega.Id, fornTeste.Id, 35),
                    CriarProduto("Eminem - Marshall Mathers LP - Vinil", 29.99m, 10, catMusica.Id, subHipHop.Id, modoEntrega.Id, fornTeste.Id, 20),
                    CriarProduto("Dr. Dre - 2001 - CD", 13.99m, 5, catMusica.Id, subHipHop.Id, modoEntrega.Id, fornTeste.Id, 28),

                    // ==================== JOGOS - PLAYSTATION (3) ====================
                    CriarProduto("God of War Ragnarök - PS5", 69.99m, 10, catJogos.Id, subPS.Id, modoEntrega.Id, fornTeste.Id, 50),
                    CriarProduto("Spider-Man 2 - PS5", 79.99m, 0, catJogos.Id, subPS.Id, modoEntrega.Id, fornTeste.Id, 60),
                    CriarProduto("The Last of Us Part II - PS4", 39.99m, 20, catJogos.Id, subPS.Id, modoEntrega.Id, fornTeste.Id, 40),

                    // ==================== JOGOS - XBOX (3) ====================
                    CriarProduto("Halo Infinite - Xbox Series X", 59.99m, 15, catJogos.Id, subXbox.Id, modoEntrega.Id, fornTeste.Id, 45),
                    CriarProduto("Forza Horizon 5 - Xbox Series X", 69.99m, 5, catJogos.Id, subXbox.Id, modoEntrega.Id, fornTeste.Id, 55),
                    CriarProduto("GTA V - Xbox Series X", 29.99m, 10, catJogos.Id, subXbox.Id, modoEntrega.Id, fornTeste.Id, 70),

                    // ==================== JOGOS - NINTENDO (3) ====================
                    CriarProduto("Zelda: Tears of the Kingdom - Switch", 69.99m, 0, catJogos.Id, subNintendo.Id, modoEntrega.Id, fornTeste.Id, 80),
                    CriarProduto("Mario Kart 8 Deluxe - Switch", 59.99m, 5, catJogos.Id, subNintendo.Id, modoEntrega.Id, fornTeste.Id, 90),
                    CriarProduto("Animal Crossing - Switch", 49.99m, 10, catJogos.Id, subNintendo.Id, modoEntrega.Id, fornTeste.Id, 65),

                    // ==================== JOGOS - PC (3) ====================
                    CriarProduto("Baldur's Gate 3 - PC", 59.99m, 0, catJogos.Id, subPC.Id, modoEntrega.Id, fornTeste.Id, 75),
                    CriarProduto("Cyberpunk 2077 - PC", 49.99m, 30, catJogos.Id, subPC.Id, modoEntrega.Id, fornTeste.Id, 40),
                    CriarProduto("Elden Ring - PC", 59.99m, 10, catJogos.Id, subPC.Id, modoEntrega.Id, fornTeste.Id, 55),

                    // ==================== ACESSÓRIOS - COMANDOS (3) ====================
                    CriarProduto("Comando PS5 DualSense - Branco", 69.99m, 5, catAcessorios.Id, subComandos.Id, modoEntrega.Id, fornTeste.Id, 60),
                    CriarProduto("Comando Xbox Series X - Preto", 59.99m, 0, catAcessorios.Id, subComandos.Id, modoEntrega.Id, fornTeste.Id, 70),
                    CriarProduto("Comando Nintendo Pro - Switch", 64.99m, 5, catAcessorios.Id, subComandos.Id, modoEntrega.Id, fornTeste.Id, 40),

                    // ==================== ACESSÓRIOS - HEADSETS (3) ====================
                    CriarProduto("Headset Sony Pulse 3D - PS5", 99.99m, 5, catAcessorios.Id, subHeadsets.Id, modoEntrega.Id, fornTeste.Id, 35),
                    CriarProduto("Headset HyperX Cloud II", 79.99m, 10, catAcessorios.Id, subHeadsets.Id, modoEntrega.Id, fornTeste.Id, 50),
                    CriarProduto("Headset Razer Kraken", 69.99m, 15, catAcessorios.Id, subHeadsets.Id, modoEntrega.Id, fornTeste.Id, 45),

                    // ==================== ACESSÓRIOS - CABOS (3) ====================
                    CriarProduto("Cabo HDMI 2.1 - 2m", 19.99m, 0, catAcessorios.Id, subCabos.Id, modoEntrega.Id, fornTeste.Id, 100),
                    CriarProduto("Cabo USB-C Carregamento Rápido", 14.99m, 5, catAcessorios.Id, subCabos.Id, modoEntrega.Id, fornTeste.Id, 120),
                    CriarProduto("Cabo Ethernet Cat6 - 5m", 12.99m, 0, catAcessorios.Id, subCabos.Id, modoEntrega.Id, fornTeste.Id, 80),

                    // ==================== COLECIONÁVEIS - FIGURAS (3) ====================
                    CriarProduto("Figura Kratos - God of War", 49.99m, 10, catColeccionaveis.Id, subFiguras.Id, modoEntrega.Id, fornTeste.Id, 15),
                    CriarProduto("Figura Link - Zelda", 39.99m, 5, catColeccionaveis.Id, subFiguras.Id, modoEntrega.Id, fornTeste.Id, 20),
                    CriarProduto("Figura Master Chief - Halo", 44.99m, 10, catColeccionaveis.Id, subFiguras.Id, modoEntrega.Id, fornTeste.Id, 18),

                    // ==================== COLECIONÁVEIS - POSTERS (3) ====================
                    CriarProduto("Poster Star Wars - A New Hope", 9.99m, 0, catColeccionaveis.Id, subPosters.Id, modoEntrega.Id, fornTeste.Id, 60),
                    CriarProduto("Poster The Last of Us", 12.99m, 5, catColeccionaveis.Id, subPosters.Id, modoEntrega.Id, fornTeste.Id, 45),
                    CriarProduto("Poster Marvel Avengers", 11.99m, 0, catColeccionaveis.Id, subPosters.Id, modoEntrega.Id, fornTeste.Id, 50),

                    // ==================== COLECIONÁVEIS - EDIÇÕES LIMITADAS (3) ====================
                    CriarProduto("Steelbook Spider-Man 2 - PS5", 29.99m, 5, catColeccionaveis.Id, subEdicoes.Id, modoEntrega.Id, fornTeste.Id, 20),
                    CriarProduto("Collector's Edition Zelda TOTK", 129.99m, 0, catColeccionaveis.Id, subEdicoes.Id, modoEntrega.Id, fornTeste.Id, 10),
                    CriarProduto("Vinil Edição Limitada - Pink Floyd", 79.99m, 10, catColeccionaveis.Id, subEdicoes.Id, modoEntrega.Id, fornTeste.Id, 8)
                };

                context.Produtos.AddRange(produtos);
                context.SaveChanges();
            }

            // ========================================
            // VENDAS DE EXEMPLO
            // ========================================
            if (!context.Vendas.Any())
            {
                var cliente = context.Clientes.Include(c => c.ApplicationUser)
                    .FirstOrDefault(c => c.ApplicationUser != null && c.ApplicationUser.Email == "cliente@teste.pt");

                if (cliente != null)
                {
                    var produtosList = context.Produtos.Take(10).ToList();

                    if (produtosList.Count >= 6)
                    {
                        // Venda 1 - Pendente
                        var venda1 = new Vendas { ClienteId = cliente.Id, Data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Estado = "Pendente" };
                        context.Vendas.Add(venda1);
                        context.SaveChanges();
                        context.LinhasVenda.AddRange(
                            new LinhasVenda { VendaId = venda1.Id, ProdutoId = produtosList[0].Id, Quantidade = 2, Preco = produtosList[0].PrecoFinal },
                            new LinhasVenda { VendaId = venda1.Id, ProdutoId = produtosList[1].Id, Quantidade = 1, Preco = produtosList[1].PrecoFinal }
                        );

                        // Venda 2 - Pendente
                        var venda2 = new Vendas { ClienteId = cliente.Id, Data = DateTime.Now.AddDays(-1).ToString("yyyy-MM-dd HH:mm:ss"), Estado = "Pendente" };
                        context.Vendas.Add(venda2);
                        context.SaveChanges();
                        context.LinhasVenda.AddRange(
                            new LinhasVenda { VendaId = venda2.Id, ProdutoId = produtosList[2].Id, Quantidade = 1, Preco = produtosList[2].PrecoFinal },
                            new LinhasVenda { VendaId = venda2.Id, ProdutoId = produtosList[3].Id, Quantidade = 3, Preco = produtosList[3].PrecoFinal }
                        );

                        // Venda 3 - Confirmada
                        var venda3 = new Vendas { ClienteId = cliente.Id, Data = DateTime.Now.AddDays(-3).ToString("yyyy-MM-dd HH:mm:ss"), Estado = "Confirmada" };
                        context.Vendas.Add(venda3);
                        context.SaveChanges();
                        context.LinhasVenda.Add(new LinhasVenda { VendaId = venda3.Id, ProdutoId = produtosList[4].Id, Quantidade = 1, Preco = produtosList[4].PrecoFinal });

                        // Venda 4 - Expedida
                        var venda4 = new Vendas { ClienteId = cliente.Id, Data = DateTime.Now.AddDays(-7).ToString("yyyy-MM-dd HH:mm:ss"), Estado = "Expedida" };
                        context.Vendas.Add(venda4);
                        context.SaveChanges();
                        context.LinhasVenda.AddRange(
                            new LinhasVenda { VendaId = venda4.Id, ProdutoId = produtosList[5].Id, Quantidade = 2, Preco = produtosList[5].PrecoFinal },
                            new LinhasVenda { VendaId = venda4.Id, ProdutoId = produtosList[0].Id, Quantidade = 1, Preco = produtosList[0].PrecoFinal }
                        );

                        // Venda 5 - Rejeitada
                        var venda5 = new Vendas { ClienteId = cliente.Id, Data = DateTime.Now.AddDays(-5).ToString("yyyy-MM-dd HH:mm:ss"), Estado = "Rejeitada" };
                        context.Vendas.Add(venda5);
                        context.SaveChanges();
                        context.LinhasVenda.Add(new LinhasVenda { VendaId = venda5.Id, ProdutoId = produtosList[1].Id, Quantidade = 1, Preco = produtosList[1].PrecoFinal });

                        context.SaveChanges();
                    }
                }
            }
        }

        private static Produtos CriarProduto(string nome, decimal precoBase, decimal percentagem, 
            int categoriaId, int subcategoriaId, int modoEntregaId, int fornecedorId, int stock)
        {
            return new Produtos
            {
                Nome = nome,
                PrecoBase = precoBase,
                Percentagem = percentagem,
                PrecoFinal = precoBase + (precoBase * (percentagem / 100)),
                Estado = "Ativo",
                Stock = stock,
                Imagem = "noproductstrans.png",
                CategoriaId = categoriaId,
                SubcategoriaId = subcategoriaId,
                ModoEntregaId = modoEntregaId,
                FornecedorId = fornecedorId
            };
        }
    }
}
