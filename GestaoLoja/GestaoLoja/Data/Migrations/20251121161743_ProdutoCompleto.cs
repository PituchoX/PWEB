using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoLoja.Migrations
{
    /// <inheritdoc />
    public partial class ProdutoCompleto : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Preco",
                table: "Produtos",
                newName: "PrecoFinal");

            migrationBuilder.AddColumn<string>(
                name: "Estado",
                table: "Produtos",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "Percentagem",
                table: "Produtos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrecoBase",
                table: "Produtos",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "Stock",
                table: "Produtos",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Estado",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Percentagem",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "PrecoBase",
                table: "Produtos");

            migrationBuilder.DropColumn(
                name: "Stock",
                table: "Produtos");

            migrationBuilder.RenameColumn(
                name: "PrecoFinal",
                table: "Produtos",
                newName: "Preco");
        }
    }
}
