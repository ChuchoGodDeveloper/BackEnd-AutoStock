using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddTokensRecuperacionPassword_HU15 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TokensRecuperacionPassword",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: false),
                    Token = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Correo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FechaExpiracion = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Usado = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    FechaUso = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TokensRecuperacionPassword", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TokensRecuperacionPassword_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TokensRecuperacionPassword_Correo",
                table: "TokensRecuperacionPassword",
                column: "Correo");

            migrationBuilder.CreateIndex(
                name: "IX_TokensRecuperacionPassword_Token",
                table: "TokensRecuperacionPassword",
                column: "Token");

            migrationBuilder.CreateIndex(
                name: "IX_TokensRecuperacionPassword_UsuarioId",
                table: "TokensRecuperacionPassword",
                column: "UsuarioId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TokensRecuperacionPassword");
        }
    }
}
