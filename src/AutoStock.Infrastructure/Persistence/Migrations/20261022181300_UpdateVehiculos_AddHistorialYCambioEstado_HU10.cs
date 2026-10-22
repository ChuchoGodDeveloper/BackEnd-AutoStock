using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehiculos_AddHistorialYCambioEstado_HU10 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "EstadoActualizadoPor",
                table: "Vehiculos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EstadoActualizadoPorUsuarioId",
                table: "Vehiculos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCambioEstado",
                table: "Vehiculos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReservadoPorNombre",
                table: "Vehiculos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ReservadoPorUsuarioId",
                table: "Vehiculos",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResponsableTestDrive",
                table: "Vehiculos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistorialEstadosVehiculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EstadoAnterior = table.Column<int>(type: "int", nullable: false),
                    EstadoNuevo = table.Column<int>(type: "int", nullable: false),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    CambiadoPor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaCambio = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialEstadosVehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialEstadosVehiculo_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosVehiculo_FechaCambio",
                table: "HistorialEstadosVehiculo",
                column: "FechaCambio");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialEstadosVehiculo_VehiculoId",
                table: "HistorialEstadosVehiculo",
                column: "VehiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialEstadosVehiculo");

            migrationBuilder.DropColumn(
                name: "EstadoActualizadoPor",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "EstadoActualizadoPorUsuarioId",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "FechaCambioEstado",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "ReservadoPorNombre",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "ReservadoPorUsuarioId",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "ResponsableTestDrive",
                table: "Vehiculos");
        }
    }
}
