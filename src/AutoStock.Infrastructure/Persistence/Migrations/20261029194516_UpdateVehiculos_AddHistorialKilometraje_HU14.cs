using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AutoStock.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVehiculos_AddHistorialKilometraje_HU14 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "FechaActualizacionKilometraje",
                table: "Vehiculos",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "KilometrajeActualizadoPor",
                table: "Vehiculos",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "KilometrajeActualizadoPorUsuarioId",
                table: "Vehiculos",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "HistorialKilometrajesVehiculo",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    VehiculoId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    KilometrajeAnterior = table.Column<int>(type: "int", nullable: false),
                    KilometrajeNuevo = table.Column<int>(type: "int", nullable: false),
                    NotasInspeccion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    MotivoCorreccion = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    UsuarioId = table.Column<int>(type: "int", nullable: true),
                    ActualizadoPor = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    FechaActualizacion = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialKilometrajesVehiculo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialKilometrajesVehiculo_Vehiculos_VehiculoId",
                        column: x => x.VehiculoId,
                        principalTable: "Vehiculos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_HistorialKilometrajesVehiculo_FechaActualizacion",
                table: "HistorialKilometrajesVehiculo",
                column: "FechaActualizacion");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialKilometrajesVehiculo_VehiculoId",
                table: "HistorialKilometrajesVehiculo",
                column: "VehiculoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "HistorialKilometrajesVehiculo");

            migrationBuilder.DropColumn(
                name: "FechaActualizacionKilometraje",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "KilometrajeActualizadoPor",
                table: "Vehiculos");

            migrationBuilder.DropColumn(
                name: "KilometrajeActualizadoPorUsuarioId",
                table: "Vehiculos");
        }
    }
}
