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

    public void Update(Usuario usuario)
    {
        _context.Usuarios.Update(usuario);
    }
}
