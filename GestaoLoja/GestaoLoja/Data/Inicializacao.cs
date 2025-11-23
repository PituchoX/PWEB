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
            const string adminEmail = "admin@frutillandia.pt";
            const string adminPass = "Admin123!";

            // Criar role
            if (!roleManager.RoleExistsAsync(adminRole).Result)
            {
                roleManager.CreateAsync(new IdentityRole(adminRole)).Wait();
            }

            // Criar utilizador admin
            var admin = userManager.FindByEmailAsync(adminEmail).Result;
            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    NomeCompleto = "Administrador",
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

            // Categorias iniciais
            if (!context.Categorias.Any())
            {
                context.Categorias.AddRange(
                    new Categorias { Nome = "Fruta" },
                    new Categorias { Nome = "Legumes" }
                );
            }

            // Modos de entrega iniciais
            if (!context.ModosEntrega.Any())
            {
                context.ModosEntrega.AddRange(
                    new ModoEntrega { Tipo = "Levantamento em loja" },
                    new ModoEntrega { Tipo = "Entrega ao domicílio" }
                );
            }

            context.SaveChanges();
        }
    }
}
