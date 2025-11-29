using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using RESTfulAPIPWeb.Entities;

namespace RESTfulAPIPWeb.Data
{
    public class AppDbContext : IdentityDbContext<ApplicationUser>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Categoria> Categorias => Set<Categoria>();
        public DbSet<Produto> Produtos => Set<Produto>();
        public DbSet<ModoEntrega> ModosEntrega => Set<ModoEntrega>();
        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
        public DbSet<Venda> Vendas => Set<Venda>();
        public DbSet<LinhaVenda> LinhasVenda => Set<LinhaVenda>();
     

    }
}
