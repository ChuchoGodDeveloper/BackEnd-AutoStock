using AutoStock.Domain.Entities;

namespace AutoStock.Application.Interfaces;

public interface IJwtTokenGenerator
{
    (string Token, string Jti, DateTime ExpiresAt) GenerateToken(Usuario usuario);
}
