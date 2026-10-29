using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehiculos_AddHistorialUbicacion_HU13 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaReubicacion",
                table: "Vehiculos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReubicadoPor",
                table: "Vehiculos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReubicadoPorUsuarioId",
                table: "Vehiculos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistorialUbicacionesVehiculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ZonaAnterior = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CajonAnterior = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    ZonaNueva = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    CajonNuevo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    ReubicadoPor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialUbicacionesVehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialUbicacionesVehiculo_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialUbicacionesVehiculo_FechaMovimiento",
                table: "HistorialUbicacionesVehiculo",
                column: "FechaMovimiento");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialUbicacionesVehiculo_VehiculoId",
                table: "HistorialUbicacionesVehiculo",
                column: "VehiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialUbicacionesVehiculo");

            migrationBuilder.DropColumn(
                name: "FechaReubicacion",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "ReubicadoPor",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "ReubicadoPorUsuarioId",
                table: "Vehiculos");
        }
    }
}
