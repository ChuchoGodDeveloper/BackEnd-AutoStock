using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehiculos_AddFichaTecnica_HU03 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColorExterior",
                table: "Vehiculos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ColorInterior",
                table: "Vehiculos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Equipamiento",
                table: "Vehiculos",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Motor",
                table: "Vehiculos",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NotasInspeccion",
                table: "Vehiculos",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NumeroStock",
                table: "Vehiculos",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Potencia",
                table: "Vehiculos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Transmision",
                table: "Vehiculos",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VIN",
                table: "Vehiculos",
                type: "nvarchar(17)",
                maxLength: 17,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "ImagenesVehiculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Url = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    EsPrincipal = table.Column<bool>(type: "bit", nullable: false),
                    Orden = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImagenesVehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ImagenesVehiculo_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_NumeroStock",
                table: "Vehiculos",
                column: "NumeroStock",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Vehiculos_VIN",
                table: "Vehiculos",
                column: "VIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ImagenesVehiculo_VehiculoId",
                table: "ImagenesVehiculo",
                column: "VehiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImagenesVehiculo");

            migrationBuilder.DropIndex(
                name: "IX_Vehiculos_NumeroStock",
                table: "Vehiculos");

            migrationBuilder.DropIndex(
                name: "IX_Vehiculos_VIN",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "ColorExterior",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "ColorInterior",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Equipamiento",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Motor",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "NotasInspeccion",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "NumeroStock",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Potencia",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "Transmision",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "VIN",
                table: "Vehiculos");
        }
    }
}
