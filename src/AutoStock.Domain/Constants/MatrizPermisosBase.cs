namespace AutoStock.Domain.Constants;

public static class MatrizPermisosBase
{
    public static bool EsPermisoActivoBase(string nombreRol, string codigoPermiso)
    {
        if (codigoPermiso == CodigosPermiso.ConsultarCatalogo)
        {
            return true;
        }

        if (nombreRol == "Administrador")
        {
            return true;
        }

        return nombreRol switch
        {
            "Asesor de Ventas" => codigoPermiso switch
            {
                CodigosPermiso.CambiarEstado => true,
                _ => false,
            },
            "Encargado de Lote y Recepción" => codigoPermiso switch
            {
                CodigosPermiso.CambiarEstado or
                CodigosPermiso.RegistrarVehiculos or
                CodigosPermiso.CapturarFotografias or
                CodigosPermiso.AsignarCajon or
                CodigosPermiso.EditarKilometrajeNotas or
                CodigosPermiso.GestionarOrdenesServicio => true,
                _ => false,
            },
            "Gerente de Ventas" => codigoPermiso switch
            {
                CodigosPermiso.CambiarEstado or
                CodigosPermiso.EditarKilometrajeNotas or
                CodigosPermiso.GestionarOrdenesServicio or
                CodigosPermiso.MarcarVendido or
                CodigosPermiso.VerReporteVentas or
                CodigosPermiso.VerMetricasLote => true,
                _ => false,
            },
            _ => false,
        };
    }
}
