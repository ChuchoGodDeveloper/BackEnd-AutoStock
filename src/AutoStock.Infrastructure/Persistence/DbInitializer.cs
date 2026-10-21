using AutoStock.Application.Interfaces;
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
            var roles = new List<Rol>
            {
                new() { Id = 1, Nombre = "Administrador", Descripcion = "Acceso total al sistema" },
                new() { Id = 2, Nombre = "Gerente de Ventas", Descripcion = "Gestión y supervisión de ventas" },
                new() { Id = 3, Nombre = "Encargado de Lote y Recepción", Descripcion = "Administración de lotes y recepción de mercancía" },
                new() { Id = 4, Nombre = "Asesor de Ventas", Descripcion = "Atención y asesoría a clientes" },
            };

            var usuarios = new List<Usuario>
            {
                new()
                {
                    EmpleadoId = "EMP-00001",
                    Correo = "admin@autostock.com",
                    NombreCompleto = "Carlos Ramírez",
                    PasswordHash = passwordHasher.Hash("AdminAutoStock2026!"),
                    RolId = 1,
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
                    PasswordHash = passwordHasher.Hash("AsesorAutoStock2026!"),
                    RolId = 4,
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
                    PasswordHash = passwordHasher.Hash("Inactivo2026!"),
                    RolId = 4,
                    Activo = false,
                    IntentosFallidos = 0,
                    BloqueadoHasta = null,
                    FechaCreacion = DateTime.UtcNow,
                },
            };

            context.Roles.AddRange(roles);
            context.Usuarios.AddRange(usuarios);

            await context.SaveChangesAsync();
        }

        if (!await context.Vehiculos.AnyAsync())
        {
            context.Vehiculos.AddRange(CrearVehiculosDePrueba());

            await context.SaveChangesAsync();
        }
    }

    private static readonly string[] ColoresExterior = ["Blanco", "Negro", "Gris", "Azul", "Rojo"];
    private static readonly string[] ColoresInterior = ["Negro", "Beige", "Gris"];

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
                    });
                }
            }

            vehiculos.Add(vehiculo);
        }

        return vehiculos;
    }
}
