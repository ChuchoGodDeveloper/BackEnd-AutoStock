using AutoStock.Domain.Entities;
using AutoStock.Domain.Interfaces;
using AutoStock.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AutoStock.Infrastructure.Repositories;

public class TokenRecuperacionRepository : ITokenRecuperacionRepository
{
    private readonly AutoStockDbContext _context;

    public TokenRecuperacionRepository(AutoStockDbContext context)
    {
        _context = context;
    }

    public async Task<TokenRecuperacionPassword?> ObtenerPorTokenAsync(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
        {
            return null;
        }

        var tokenNormalizado = token.Trim();
        return await _context.TokensRecuperacionPassword
            .Include(t => t.Usuario)
            .FirstOrDefaultAsync(t => t.Token == tokenNormalizado);
    }

    public async Task<IReadOnlyList<TokenRecuperacionPassword>> ObtenerActivosPorUsuarioIdAsync(int usuarioId)
    {
        var ahora = DateTime.UtcNow;
        return await _context.TokensRecuperacionPassword
            .Where(t => t.UsuarioId == usuarioId && !t.Usado && t.FechaExpiracion > ahora)
            .ToListAsync();
    }

    public async Task<int> ContarSolicitudesRecientesPorCorreoAsync(string correo, DateTime desde)
    {
        if (string.IsNullOrWhiteSpace(correo))
        {
            return 0;
        }

        var correoNormalizado = correo.Trim().ToLowerInvariant();
        return await _context.TokensRecuperacionPassword
            .Where(t => t.Correo.ToLower() == correoNormalizado && t.FechaCreacion >= desde)
            .CountAsync();
    }

    public async Task AgregarAsync(TokenRecuperacionPassword tokenRecuperacion)
    {
        await _context.TokensRecuperacionPassword.AddAsync(tokenRecuperacion);
    }

    public void Actualizar(TokenRecuperacionPassword tokenRecuperacion)
    {
        _context.TokensRecuperacionPassword.Update(tokenRecuperacion);
    }
}
