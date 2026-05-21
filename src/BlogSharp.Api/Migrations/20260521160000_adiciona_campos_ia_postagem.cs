using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BlogSharp.Api.Migrations
{
    /// <inheritdoc />
    public partial class AdicionaCamposIAPostagem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CategoriaIA",
                table: "Postagens",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumoIA",
                table: "Postagens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TagsIA",
                table: "Postagens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CategoriaIA",
                table: "Postagens");

            migrationBuilder.DropColumn(
                name: "ResumoIA",
                table: "Postagens");

            migrationBuilder.DropColumn(
                name: "TagsIA",
                table: "Postagens");
        }
    }
}
