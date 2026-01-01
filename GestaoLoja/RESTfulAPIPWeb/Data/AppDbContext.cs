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

        // DbSets - nomes das tabelas correspondem à BD do GestaoLoja
        public DbSet<Categoria> Categorias { get; set; } = default!;
        public DbSet<Subcategoria> Subcategorias { get; set; } = default!;
        public DbSet<Produto> Produtos { get; set; } = default!;
        public DbSet<ModoEntrega> ModosEntrega { get; set; } = default!;
        public DbSet<Cliente> Clientes { get; set; } = default!;
        public DbSet<Fornecedor> Fornecedores { get; set; } = default!;
        public DbSet<Venda> Vendas { get; set; } = default!;
        public DbSet<LinhaVenda> LinhasVenda { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configurar relações para corresponder à BD existente
            builder.Entity<Cliente>()
                .HasOne(c => c.ApplicationUser)
                .WithOne()
                .HasForeignKey<Cliente>(c => c.ApplicationUserId);

            builder.Entity<Fornecedor>()
                .HasOne(f => f.ApplicationUser)
                .WithOne()
                .HasForeignKey<Fornecedor>(f => f.ApplicationUserId);

            builder.Entity<Produto>()
                .HasOne(p => p.Categoria)
                .WithMany(c => c.Produtos)
                .HasForeignKey(p => p.CategoriaId);

            // Configurar relação Subcategoria -> Categoria
            builder.Entity<Subcategoria>()
                .HasOne(s => s.Categoria)
                .WithMany(c => c.Subcategorias)
                .HasForeignKey(s => s.CategoriaId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configurar relação Produto -> Subcategoria (opcional)
            builder.Entity<Produto>()
                .HasOne(p => p.Subcategoria)
                .WithMany(s => s.Produtos)
                .HasForeignKey(p => p.SubcategoriaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.Entity<Produto>()
                .HasOne(p => p.Fornecedor)
                .WithMany(f => f.Produtos)
                .HasForeignKey(p => p.FornecedorId);

            builder.Entity<Produto>()
                .HasOne(p => p.ModoEntrega)
                .WithMany()
                .HasForeignKey(p => p.ModoEntregaId);

            builder.Entity<Venda>()
                .HasOne(v => v.Cliente)
                .WithMany(c => c.Vendas)
                .HasForeignKey(v => v.ClienteId);

            builder.Entity<LinhaVenda>()
                .HasOne(l => l.Venda)
                .WithMany(v => v.LinhasVenda)
                .HasForeignKey(l => l.VendaId);

            builder.Entity<LinhaVenda>()
                .HasOne(l => l.Produto)
                .WithMany()
                .HasForeignKey(l => l.ProdutoId);
        }
    }
}
