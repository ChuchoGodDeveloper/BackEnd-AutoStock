using AutoStock.Application.DTOs;

namespace AutoStock.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(LoginRequestDto request);

    Task LogoutAsync(string jti);

    Task<PerfilResponseDto> GetPerfilActualAsync(int usuarioId);
}
