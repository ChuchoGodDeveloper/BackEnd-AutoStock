using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoStock.Application.Interfaces;
using AutoStock.Domain.Entities;
using AutoStock.Infrastructure.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace AutoStock.Infrastructure.Authentication;

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private const int HorasVigencia = 8;

    private readonly JwtSettings _jwtSettings;

    public JwtTokenGenerator(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public (string Token, string Jti, DateTime ExpiresAt) GenerateToken(Usuario usuario)
    {
        var jti = Guid.NewGuid().ToString();
        var expiresAt = DateTime.UtcNow.AddHours(HorasVigencia);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, jti),
            new("empleadoId", usuario.EmpleadoId),
            new(JwtRegisteredClaimNames.Email, usuario.Correo),
            new("nombre", usuario.NombreCompleto),
            new(ClaimTypes.Role, usuario.Rol.Nombre),
            new("role", usuario.Rol.Nombre),
        };

        var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.SecretKey));
        var credentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _jwtSettings.Issuer,
            audience: _jwtSettings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return (tokenString, jti, expiresAt);
    }
}
