using AutoStock.Application.Interfaces;
using AutoStock.Domain.Constants;
using AutoStock.Domain.Entities;
using AutoStock.Domain.Enums;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Persistence;

public static class DbInitializer
{
    private static readonly (string Marca, string Modelo, int Anio, decimal Precio, int Kilometraje, EstadoOperativo Estado)[] CatalogoPrueba =
    {
        ("Toyota", "Corolla", 2024, 389900m, 18500, EstadoOperativo.Disponible),
        ("Toyota", "RAV4", 2025, 549900m, 4200, EstadoOperativo.Disponible),
        ("Nissan", "Versa", 2024, 279900m, 22000, EstadoOperativo.Disponible),
        ("Mazda", "CX-5", 2023, 489900m, 35000, EstadoOperativo.TestDrive),
        ("Chevrolet", "Aveo", 2023, 249900m, 41000, EstadoOperativo.TestDrive),
        ("Kia", "Sportage", 2024, 512900m, 12000, EstadoOperativo.EnTaller),
        ("Hyundai", "Elantra", 2023, 315900m, 28000, EstadoOperativo.EnTaller),
        ("Volkswagen", "Jetta", 2024, 379900m, 15500, EstadoOperativo.Disponible),
        ("Honda", "CR-V", 2024, 565000m, 9000, EstadoOperativo.Reservado),
        ("Ford", "Maverick", 2024, 529000m, 6000, EstadoOperativo.Disponible),
        ("Renault", "Kwid", 2023, 199900m, 33000, EstadoOperativo.EnLavado),
        ("Suzuki", "Swift", 2023, 229900m, 31000, EstadoOperativo.Disponible),
        ("MG", "ZS", 2024, 299900m, 8000, EstadoOperativo.TestDrive),
        ("Jeep", "Compass", 2023, 499900m, 26000, EstadoOperativo.EnTaller),
        ("Subaru", "Forester", 2024, 545000m, 11000, EstadoOperativo.Disponible),
        ("Nissan", "Kicks", 2024, 319900m, 14000, EstadoOperativo.Disponible),
        ("Toyota", "Tacoma", 2023, 689000m, 45000, EstadoOperativo.Reservado),
        ("Chevrolet", "Tracker", 2024, 339900m, 10000, EstadoOperativo.EnLavado),
        ("Kia", "Rio", 2022, 259900m, 58000, EstadoOperativo.Vendido),
        ("Hyundai", "Tucson", 2024, 539900m, 7000, EstadoOperativo.Disponible),
        ("Mazda", "3", 2023, 369900m, 24000, EstadoOperativo.TestDrive),
        ("Volkswagen", "Tiguan", 2024, 559900m, 9500, EstadoOperativo.Disponible),
        ("Honda", "Civic", 2024, 415000m, 13000, EstadoOperativo.Disponible),
        ("Ford", "Explorer", 2022, 749000m, 62000, EstadoOperativo.Vendido),
        ("Toyota", "Yaris", 2023, 269900m, 27000, EstadoOperativo.Disponible),
    };

    public static async Task SeedAsync(AutoStockDbContext context, IPasswordHasher passwordHasher)
    {
        if (!await context.Roles.AnyAsync())
        {
            var rolAdmin = new Rol { Nombre = "Administrador", Descripcion = "Acceso total al sistema" };
            var rolGerente = new Rol { Nombre = "Gerente de Ventas", Descripcion = "Gestión y supervisión de ventas" };
            var rolEncargado = new Rol { Nombre = "Encargado de Lote y Recepción", Descripcion = "Administración de lotes y recepción de mercancía" };
            var rolAsesor = new Rol { Nombre = "Asesor de Ventas", Descripcion = "Atención y asesoría a clientes" };

            var usuarios = new List<Usuario>
            {
                new()
                {
                    EmpleadoId = "EMP-00001",
                    Correo = "admin@autostock.com",
                    NombreCompleto = "Carlos Ramírez",
                    FechaNacimiento = new DateOnly(1985, 5, 20),
                    PasswordHash = passwordHasher.Hash("AdminAutoStock2026!"),
                    Rol = rolAdmin,
                    Activo = true,
                    IntentosFallidos = 0,
                    BloqueadoHasta = null,
                    FechaCreacion = DateTime.UtcNow,
                },
                new()
                {
                    EmpleadoId = "EMP-00421",
                    Correo = "asesor@autostock.com",
                    NombreCompleto = "Marcos Hernández",
                    FechaNacimiento = new DateOnly(1992, 8, 14),
                    PasswordHash = passwordHasher.Hash("AsesorAutoStock2026!"),
                    Rol = rolAsesor,
                    Activo = true,
                    IntentosFallidos = 0,
                    BloqueadoHasta = null,
                    FechaCreacion = DateTime.UtcNow,
                },
                new()
                {
                    EmpleadoId = "EMP-00999",
                    Correo = "inactivo@autostock.com",
                    NombreCompleto = "Usuario Inactivo",
                    FechaNacimiento = new DateOnly(1990, 1, 1),
                    PasswordHash = passwordHasher.Hash("Inactivo2026!"),
                    Rol = rolAsesor,
                    Activo = false,
                    IntentosFallidos = 0,
                    BloqueadoHasta = null,
                    FechaCreacion = DateTime.UtcNow,
                },
            };

            context.Roles.AddRange(rolAdmin, rolGerente, rolEncargado, rolAsesor);
            context.Usuarios.AddRange(usuarios);

            await context.SaveChangesAsync();
        }

        if (!await context.Permisos.AnyAsync())
        {
            var permisos = new List<Permiso>
            {
                new() { Codigo = CodigosPermiso.ConsultarCatalogo, Nombre = "Consultar catálogo y ficha técnica", Descripcion = "Permite ver el inventario completo y consultar especificaciones técnicas.", Modulo = "Catálogo", EsFijo = true },
                new() { Codigo = CodigosPermiso.CambiarEstado, Nombre = "Cambiar estado operativo", Descripcion = "Permite cambiar el estado de la unidad (disponible, taller, lavado, test drive, apartado).", Modulo = "Inventario", EsFijo = false },
                new() { Codigo = CodigosPermiso.RegistrarVehiculos, Nombre = "Registrar vehículos", Descripcion = "Permite dar de alta nuevas unidades en el lote.", Modulo = "Inventario", EsFijo = false },
                new() { Codigo = CodigosPermiso.CapturarFotografias, Nombre = "Capturar fotografías", Descripcion = "Permite subir o tomar fotografías para la evidencia del vehículo.", Modulo = "Inspección", EsFijo = false },
                new() { Codigo = CodigosPermiso.AsignarCajon, Nombre = "Asignar o reubicar cajón", Descripcion = "Permite asignar o cambiar la zona y cajón de lote del vehículo.", Modulo = "Lote", EsFijo = false },
                new() { Codigo = CodigosPermiso.EditarKilometrajeNotas, Nombre = "Editar kilometraje y notas", Descripcion = "Permite modificar el odómetro y las notas de inspección del vehículo.", Modulo = "Inspección", EsFijo = false },
                new() { Codigo = CodigosPermiso.GestionarOrdenesServicio, Nombre = "Gestionar órdenes de servicio", Descripcion = "Permite crear y dar seguimiento a órdenes de servicio mecánico o preparación.", Modulo = "Taller", EsFijo = false },
                new() { Codigo = CodigosPermiso.MarcarVendido, Nombre = "Marcar unidades como vendidas", Descripcion = "Permite registrar formalmente la venta de una unidad.", Modulo = "Ventas", EsFijo = false },
                new() { Codigo = CodigosPermiso.VerReporteVentas, Nombre = "Ver el reporte de ventas", Descripcion = "Permite consultar e imprimir el reporte comercial de ventas.", Modulo = "Ventas", EsFijo = false },
                new() { Codigo = CodigosPermiso.VerMetricasLote, Nombre = "Ver las métricas del lote", Descripcion = "Permite visualizar los indicadores clave y estadísticas del lote.", Modulo = "Dashboard", EsFijo = false },
            };

            context.Permisos.AddRange(permisos);
            await context.SaveChangesAsync();

            var roles = await context.Roles.ToListAsync();
            var rolPermisos = new List<RolPermiso>();
            var ahora = DateTime.UtcNow;

            foreach (var rol in roles)
            {
                foreach (var permiso in permisos)
                {
                    bool activo = EsPermisoActivoBase(rol.Nombre, permiso.Codigo);
                    rolPermisos.Add(new RolPermiso
                    {
                        RolId = rol.Id,
                        PermisoId = permiso.Id,
                        Activo = activo,
                        FechaModificacion = ahora,
                        ModificadoPorNombre = "Sistema (Matriz Base)",
                    });
                }
            }

            context.RolPermisos.AddRange(rolPermisos);
            await context.SaveChangesAsync();
        }

        if (!await context.Vehiculos.AnyAsync())
        {
            context.Vehiculos.AddRange(CrearVehiculosDePrueba());

            await context.SaveChangesAsync();
        }
    }

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

    private static readonly string[] ColoresExterior = ["Blanco", "Negro", "Gris", "Azul", "Rojo"];
    private static readonly string[] ColoresInterior = ["Negro", "Beige", "Gris"];
    private static readonly string[] TiposTomaSemilla =
    [
        "Frontal",
        "LateralDerecha",
        "Trasera",
        "TableroOdometro",
        "Adicional1",
        "Adicional2",
    ];

    private static List<Vehiculo> CrearVehiculosDePrueba()
    {
        var fechaBase = new DateTime(2026, 10, 20, 9, 0, 0, DateTimeKind.Utc);
        var vehiculos = new List<Vehiculo>(CatalogoPrueba.Length);

        for (var indice = 0; indice < CatalogoPrueba.Length; indice++)
        {
            var dato = CatalogoPrueba[indice];
            var sinFoto = indice % 5 == 2;
            var sinPlaca = indice % 7 == 3;
            var sinFichaTecnica = indice % 5 == 2;
            var cilindros = (indice % 2) == 0 ? "4" : "6";

            var vehiculo = new Vehiculo
            {
                Id = Guid.Parse($"00000000-0000-0000-0000-{(indice + 1).ToString("D12")}"),
                Marca = dato.Marca,
                Modelo = dato.Modelo,
                Anio = dato.Anio,
                Precio = dato.Precio,
                Kilometraje = dato.Kilometraje,
                Placa = sinPlaca ? null : $"ABC-{1000 + indice}",
                ZonaLote = $"Z{(indice % 4) + 1}",
                CajonLote = $"{(char)('A' + (indice % 6))}{(indice % 12) + 1:00}",
                FotoPrincipalUrl = sinFoto
                    ? null
                    : $"https://cdn.autostock.mx/vehiculos/{dato.Marca}-{dato.Modelo}-{dato.Anio}.jpg".ToLowerInvariant(),
                Estado = dato.Estado,
                Tipo = indice % 3 == 0 ? TipoVehiculo.Nuevo : TipoVehiculo.Seminuevo,
                FechaRegistro = fechaBase.AddMinutes(-indice * 53),
                VIN = $"3AUTST{(dato.Anio % 100):D2}{(indice + 1):D9}",
                NumeroStock = $"STK-{(indice + 1):D4}",
                Motor = sinFichaTecnica
                    ? null
                    : $"{(indice % 4) + 1}.{(indice * 3) % 10}L {cilindros} cilindros",
                Potencia = sinFichaTecnica ? null : $"{100 + (indice % 8) * 25} HP",
                Transmision = sinFichaTecnica ? null : ((indice % 2) == 0 ? "Automática" : "Manual"),
                ColorExterior = sinFichaTecnica ? null : ColoresExterior[indice % ColoresExterior.Length],
                ColorInterior = sinFichaTecnica ? null : ColoresInterior[indice % ColoresInterior.Length],
                Equipamiento = sinFichaTecnica
                    ? null
                    : "Aire acondicionado, dirección asistida, ventanillas eléctricas, frenos ABS, airbags, sistema de audio con pantalla táctil y cámara de reversa",
                NotasInspeccion = sinFichaTecnica
                    ? null
                    : $"Inspección completada el {fechaBase.AddDays(-indice):dd/MM/yyyy}. Sin daños estructurales ni reparaciones previas relevantes.",
            };

            if (!sinFichaTecnica)
            {
                var cantidadImagenes = 2 + (indice % 5);

                for (var numeroImagen = 0; numeroImagen < cantidadImagenes; numeroImagen++)
                {
                    vehiculo.Imagenes.Add(new ImagenVehiculo
                    {
                        Id = Guid.Parse($"00000000-0000-0000-0001-{(indice + 1).ToString("D6")}{(numeroImagen + 1).ToString("D6")}"),
                        VehiculoId = vehiculo.Id,
                        Url = numeroImagen == 0
                            ? $"https://cdn.autostock.mx/vehiculos/{dato.Marca}-{dato.Modelo}-{dato.Anio}.jpg".ToLowerInvariant()
                            : $"https://cdn.autostock.mx/vehiculos/{dato.Marca}-{dato.Modelo}-{dato.Anio}-v{numeroImagen + 1}.jpg".ToLowerInvariant(),
                        EsPrincipal = numeroImagen == 0,
                        Orden = numeroImagen,
                        TipoToma = TiposTomaSemilla[numeroImagen % TiposTomaSemilla.Length],
                        FechaCaptura = vehiculo.FechaRegistro,
                        CapturadaPor = "Carlos Ramírez",
                    });
                }
            }

            vehiculos.Add(vehiculo);
        }

        return vehiculos;
    }
}
