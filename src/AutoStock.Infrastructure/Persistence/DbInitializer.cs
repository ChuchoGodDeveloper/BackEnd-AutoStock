using AutoStock.Application.Interfaces;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Persistence;

public static class DbInitializer
{
    public static async Task SeedAsync(AutoStockDbContext context, IPasswordHasher passwordHasher)
    {
        if (await context.Roles.AnyAsync())
        {
            return;
        }

        var roles = new List<Rol>
        {
            new() { Id = 1, Nombre = "Administrador", Descripcion = "Acceso total al sistema" },
            new() { Id = 2, Nombre = "Gerente de Ventas", Descripcion = "Gestión y supervisión de ventas" },
            new() { Id = 3, Nombre = "Encargado de Lote y Recepción", Descripcion = "Administración de lotes y recepción de mercancía" },
            new() { Id = 4, Nombre = "Asesor de Ventas", Descripcion = "Atención y asesoría a clientes" },
        };

        context.Roles.AddRange(roles);

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

        context.Usuarios.AddRange(usuarios);

        await context.SaveChangesAsync();
    }
}
