namespace AutoStock.Application.Interfaces;

public interface IEmailService
{
    Task EnviarEmailRecuperacionPasswordAsync(
        string correoDestinatario,
        string nombreDestinatario,
        string enlaceRecuperacion);
}
