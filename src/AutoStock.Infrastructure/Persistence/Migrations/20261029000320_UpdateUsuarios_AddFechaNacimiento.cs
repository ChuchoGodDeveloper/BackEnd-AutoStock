using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateUsuarios_AddFechaNacimiento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "FechaNacimiento",
                table: "Usuarios",
                type: "date",
                nullable: false,
                defaultValue: new DateOnly(2000, 1, 1));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Usuarios");
        }
    }
}
