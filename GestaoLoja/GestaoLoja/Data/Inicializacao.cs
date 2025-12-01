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

            const string adminRole = "Administrador";
            const string adminEmail = "admin@gestao.pt";     // <-- NOVO EMAIL
            const string adminPass = "Admin123!";            // <-- NOVA PASSWORD

            // Criar role Administrador se não existir
            if (!roleManager.RoleExistsAsync(adminRole).Result)
            {
                roleManager.CreateAsync(new IdentityRole(adminRole)).Wait();
            }

            // Criar utilizador administrador
            var admin = userManager.FindByEmailAsync(adminEmail).Result;
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Administrador do Sistema",
                    Estado = "Ativo",       // <-- ATIVO POR DEFINIÇÃO
                    Perfil = "Administrador", // <-- PERFIL DEFINIDO
                    EmailConfirmed = true
                };

                var result = userManager.CreateAsync(admin, adminPass).Result;

                if (!result.Succeeded)
                {
                    throw new Exception("Erro a criar admin: " +
                        string.Join(", ", result.Errors.Select(e => e.Description)));
                }

                userManager.AddToRoleAsync(admin, adminRole).Wait();
            }
            // Criar fornecedor interno (caso não exista)
            var fornecedorInterno = context.Fornecedores
                .FirstOrDefault(f => f.NomeEmpresa == "Fornecedor Interno");

            if (fornecedorInterno == null)
            {
                fornecedorInterno = new Fornecedor
                {
                    NomeEmpresa = "Fornecedor Interno",
                    NIF = "000000000",
                    Estado = "Aprovado",
                    ApplicationUserId = admin.Id  // o admin é o dono deste fornecedor interno
                };

                context.Fornecedores.Add(fornecedorInterno);
                context.SaveChanges();
            }


            // Modos de entrega iniciais
            if (!context.ModosEntrega.Any())
            {
                context.ModosEntrega.AddRange(
                    new ModoEntrega
                    {
                        Nome = "Levantamento em loja",
                        Tipo = "Levantamento em loja",
                        Detalhe = ""
                    },
                    new ModoEntrega
                    {
                        Nome = "Entrega ao domicílio",
                        Tipo = "Entrega ao domicílio",
                        Detalhe = ""
                    }
                );
            }

            context.SaveChanges();
        }
    }
}
