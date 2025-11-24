using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestaoLoja.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddImagemToCategorias : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Imagem",
                table: "Categorias",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Imagem",
                table: "Categorias");
        }
    }
}
