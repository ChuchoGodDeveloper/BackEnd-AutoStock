namespace AutoStock.Domain.Constants;

public static class CodigosPermiso
{
    public const string ConsultarCatalogo = "consultar_catalogo";
    public const string CambiarEstado = "cambiar_estado";
    public const string RegistrarVehiculos = "registrar_vehiculos";
    public const string CapturarFotografias = "capturar_fotografias";
    public const string AsignarCajon = "asignar_cajon";
    public const string EditarKilometrajeNotas = "editar_kilometraje_notas";
    public const string GestionarOrdenesServicio = "gestionar_ordenes_servicio";
    public const string MarcarVendido = "marcar_vendido";
    public const string VerReporteVentas = "ver_reporte_ventas";
    public const string VerMetricasLote = "ver_metricas_lote";

    public static readonly string[] TodosLosCodigos =
    [
        ConsultarCatalogo,
        CambiarEstado,
        RegistrarVehiculos,
        CapturarFotografias,
        AsignarCajon,
        EditarKilometrajeNotas,
        GestionarOrdenesServicio,
        MarcarVendido,
        VerReporteVentas,
        VerMetricasLote,
    ];
}
