using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly AutoStockDbContext _context;

    public UsuarioRepository(AutoStockDbContext context)
    {
        _context = context;
    }

    public async Task<Usuario?> GetByIdentifierAsync(string identificador)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u =>
                u.EmpleadoId.ToLower() == identificador ||
                u.Correo.ToLower() == identificador);
    }

    public async Task<Usuario?> GetByIdAsync(int id)
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<bool> ExisteEmpleadoIdOCorreoAsync(string empleadoId, string correo)
    {
        var empleadoNormalizado = empleadoId.Trim().ToLower();
        var correoNormalizado = correo.Trim().ToLower();

        return await _context.Usuarios.AnyAsync(u =>
            u.EmpleadoId.ToLower() == empleadoNormalizado ||
            u.Correo.ToLower() == correoNormalizado);
    }

    public async Task<bool> ExisteEmpleadoIdAsync(string empleadoId)
    {
        var empleadoNormalizado = empleadoId.Trim().ToLower();
        return await _context.Usuarios.AnyAsync(u => u.EmpleadoId.ToLower() == empleadoNormalizado);
    }

    public async Task AddAsync(Usuario usuario)
    {
        await _context.Usuarios.AddAsync(usuario);
    }

    public async Task<IReadOnlyList<Usuario>> GetAllAsync()
    {
        return await _context.Usuarios
            .Include(u => u.Rol)
            .OrderBy(u => u.NombreCompleto)
            .ToListAsync();
    }

    public void Update(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
    }
}
