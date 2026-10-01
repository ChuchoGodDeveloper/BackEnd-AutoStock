using AutoStock.DTOs;

namespace AutoStock.Interfaces;

public interface ICatalogoService
{
    Task<PaginacionDto<VehiculoCatalogoDto>> ObtenerCatalogoAsync(int pagina);
}
