using AutoStock.Domain.Entities;

namespace AutoStock.Domain.Interfaces;

public interface IVehiculoRepository
{
    Task<IReadOnlyList<Vehiculo>> ObtenerTodosAsync();

    Task<Vehiculo?> ObtenerPorIdAsync(Guid id);

    Task<Vehiculo?> ObtenerPorVinAsync(string vin);

    Task AgregarAsync(Vehiculo vehiculo);

    Task GuardarImagenAsync(
        Vehiculo vehiculo,
        ImagenVehiculo nuevaImagen,
        ImagenVehiculo? imagenReemplazada = null);

    Task EliminarImagenAsync(Vehiculo vehiculo, ImagenVehiculo imagen);

    Task RegistrarCambioEstadoAsync(
        Vehiculo vehiculo,
        HistorialEstadoVehiculo registroHistorial);

    Task<Vehiculo?> ObtenerOcupanteCajonAsync(
        string zona,
        string cajon,
        Guid? ignorarVehiculoId = null);

    Task RegistrarReubicacionAsync(
        Vehiculo vehiculo,
        HistorialUbicacionVehiculo registroHistorial);

    Task RegistrarActualizacionKilometrajeAsync(
        Vehiculo vehiculo,
        HistorialKilometrajeVehiculo registroHistorial);

    Task RegistrarVentaAsync(
        Vehiculo vehiculo,
        Venta venta,
        HistorialEstadoVehiculo registroHistorial);

    Task RevertirVentaAsync(
        Vehiculo vehiculo,
        Venta venta,
        HistorialEstadoVehiculo registroEstado,
        HistorialUbicacionVehiculo registroUbicacion);

    Task<IReadOnlyList<HistorialEstadoVehiculo>> ObtenerUltimosMovimientosAsync(int cantidad = 5);

    Task<IReadOnlyList<Venta>> ObtenerVentasMesActualAsync();

    Task<IReadOnlyList<Venta>> ObtenerVentasPorRangoAsync(DateTime fechaInicio, DateTime fechaFin);
}
