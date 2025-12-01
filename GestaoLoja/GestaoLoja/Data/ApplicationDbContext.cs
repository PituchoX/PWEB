using GestaoLoja.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace GestaoLoja.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categorias> Categorias { get; set; }
        public DbSet<Produtos> Produtos { get; set; }
        public DbSet<ModoEntrega> ModosEntrega { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<Fornecedor> Fornecedores { get; set; }
        public DbSet<Vendas> Vendas { get; set; }
        public DbSet<LinhasVenda> LinhasVenda { get; set; }

    }
}
