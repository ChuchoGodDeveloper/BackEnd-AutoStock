using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoStock.Data;
using AutoStock.DTOs;
using AutoStock.Interfaces;
using AutoStock.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace AutoStock.Services;

public class AuthService : IAuthService
{
    private readonly AutoStockDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthService(AutoStockDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Rol)
            .FirstOrDefaultAsync(u => u.IdEmpleado == request.Credencial || u.Correo == request.Credencial);

        if (usuario == null)
        {
            throw new UnauthorizedAccessException("ID de empleado o contraseña incorrectos");
        }

        if (!usuario.Activo)
        {
            throw new InvalidOperationException("Tu cuenta está desactivada. Contacta al administrador");
        }

        if (usuario.BloqueadoHasta.HasValue && usuario.BloqueadoHasta.Value > DateTime.UtcNow)
        {
            throw new InvalidOperationException("Tu cuenta está bloqueada temporalmente. Intenta más tarde");
        }

        if (!BCrypt.Net.BCrypt.Verify(request.Password, usuario.PasswordHash))
        {
            usuario.IntentosFallidos++;

            if (usuario.IntentosFallidos >= 5)
            {
                usuario.BloqueadoHasta = DateTime.UtcNow.AddMinutes(15);
                usuario.IntentosFallidos = 0;
                await _context.SaveChangesAsync();
                throw new UnauthorizedAccessException("ID de empleado o contraseña incorrectos");
            }

            await _context.SaveChangesAsync();
            throw new UnauthorizedAccessException("ID de empleado o contraseña incorrectos");
        }

        usuario.IntentosFallidos = 0;
        usuario.BloqueadoHasta = null;

        var token = GenerateJwtToken(usuario);

        var sesion = new Sesion
        {
            UsuarioId = usuario.Id,
            Token = token,
            FechaCreacion = DateTime.UtcNow,
            FechaExpiracion = DateTime.UtcNow.AddHours(8),
            Activa = true
        };

        _context.Sesiones.Add(sesion);
        await _context.SaveChangesAsync();

        return new LoginResponseDto
        {
            Token = token,
            NombreCompleto = usuario.NombreCompleto,
            Rol = usuario.Rol.Nombre,
            RequiereCambioPassword = usuario.RequiereCambioPassword
        };
    }

    private string GenerateJwtToken(Usuario usuario)
    {
        var jwtSettings = _configuration.GetSection("JwtSettings");
        var secretKey = jwtSettings["SecretKey"] ?? throw new InvalidOperationException("JWT SecretKey no configurada");
        var issuer = jwtSettings["Issuer"] ?? "AutoStock";
        var audience = jwtSettings["Audience"] ?? "AutoStock";

        var claims = new[]
        {
            new Claim(ClaimTypes.Name, usuario.NombreCompleto),
            new Claim(ClaimTypes.Role, usuario.Rol.Nombre),
            new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
            new Claim("IdEmpleado", usuario.IdEmpleado)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(8),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}
