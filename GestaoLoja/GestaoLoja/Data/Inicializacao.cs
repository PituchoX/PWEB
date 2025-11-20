using GestaoLoja.Entities;
using Microsoft.EntityFrameworkCore;

namespace GestaoLoja.Data
{
    public static class Inicializacao
    {
        public static void SeedDatabase(IServiceProvider services)
        {
            using var scope = services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Garante que a BD existe / aplica migrations
            context.Database.Migrate();

            // EXEMPLO de seed básico (podes ajustar conforme o professor)
            if (!context.Categorias.Any())
            {
                var catFruta = new Categorias { Nome = "Fruta" };
                var catLegumes = new Categorias { Nome = "Legumes" };

                context.Categorias.AddRange(catFruta, catLegumes);
                context.SaveChanges();
            }

            if (!context.ModosEntrega.Any())
            {
                context.ModosEntrega.AddRange(
                    new ModoEntrega { Tipo = "Levantamento em loja" },
                    new ModoEntrega { Tipo = "Entrega ao domicílio" }
                );
                context.SaveChanges();
            }

            // Produtos podes deixar para mais tarde, quando tiveres as imagens, se quiseres
        }
    }
}

