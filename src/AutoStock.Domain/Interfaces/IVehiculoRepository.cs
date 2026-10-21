using AutoStock.Domain.Entities;

namespace AutoStock.Domain.Interfaces;

public interface IVehiculoRepository
{
    Task<IReadOnlyList<Vehiculo>> ObtenerTodosAsync();
}
