using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebApiAutores.Migrations
{
    public partial class authorimage : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
            name: "Imagen",
            table: "Autores",
            type: "text",
            nullable: true);

        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
