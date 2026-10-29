using System.Net;
using System.Net.Mail;
using AutoStock.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace AutoStock.Infrastructure.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnviarEmailRecuperacionPasswordAsync(
        string correoDestinatario,
        string nombreDestinatario,
        string enlaceRecuperacion)
    {
        var asunto = "Restablece tu contraseña - AutoStock";
        var cuerpoHtml = GenerarPlantillaRecuperacionHtml(nombreDestinatario, enlaceRecuperacion);

        var host = _configuration["SmtpSettings:Host"];
        if (string.IsNullOrWhiteSpace(host))
        {
            _logger.LogInformation(
                "[MODO SIMULACIÓN / LOG] EnviarEmailRecuperacionPasswordAsync a {Correo}. Enlace: {Enlace}",
                correoDestinatario,
                enlaceRecuperacion);
            return;
        }

        try
        {
            var port = int.TryParse(_configuration["SmtpSettings:Port"], out var p) ? p : 587;
            var enableSsl = bool.TryParse(_configuration["SmtpSettings:EnableSsl"], out var ssl) && ssl;
            var userName = _configuration["SmtpSettings:UserName"];
            var password = _configuration["SmtpSettings:Password"];
            var fromEmail = _configuration["SmtpSettings:FromEmail"] ?? "no-reply@autostock.mx";
            var fromName = _configuration["SmtpSettings:FromName"] ?? "AutoStock Seguridad";

            using var mensaje = new MailMessage
            {
                From = new MailAddress(fromEmail, fromName),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true,
            };
            mensaje.To.Add(new MailAddress(correoDestinatario, nombreDestinatario));

            using var clienteSmtp = new SmtpClient(host, port)
            {
                EnableSsl = enableSsl,
            };

            if (!string.IsNullOrWhiteSpace(userName) && !string.IsNullOrWhiteSpace(password))
            {
                clienteSmtp.Credentials = new NetworkCredential(userName, password);
            }

            await clienteSmtp.SendMailAsync(mensaje);

            _logger.LogInformation(
                "Correo de recuperación enviado exitosamente a {Correo}",
                correoDestinatario);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(
                ex,
                "No fue posible enviar el correo vía SMTP a {Correo}. Fallback a log. Enlace: {Enlace}",
                correoDestinatario,
                enlaceRecuperacion);
        }
    }

    private static string GenerarPlantillaRecuperacionHtml(string nombre, string enlace)
    {
        return $@"
<!DOCTYPE html>
<html lang=""es"">
<head>
  <meta charset=""UTF-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
  <title>Restablecer contraseña - AutoStock</title>
</head>
<body style=""font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif; background-color: #f8fafc; margin: 0; padding: 24px; color: #0f172a;"">
  <table role=""presentation"" width=""100%"" border=""0"" cellspacing=""0"" cellpadding=""0"" style=""max-width: 580px; margin: 0 auto; background-color: #ffffff; border-radius: 20px; overflow: hidden; border: 1px solid #e2e8f0; box-shadow: 0 4px 6px -1px rgba(0,0,0,0.05);"">
    <tr>
      <td style=""background-color: #1e3a8a; padding: 28px 32px; text-align: left;"">
        <h1 style=""color: #ffffff; margin: 0; font-size: 22px; font-weight: 800; letter-spacing: -0.5px;"">AutoStock</h1>
        <p style=""color: #bfdbfe; margin: 4px 0 0 0; font-size: 13px; font-weight: 500;"">Sistema de Gestión de Inventario</p>
      </td>
    </tr>
    <tr>
      <td style=""padding: 32px;"">
        <h2 style=""font-size: 18px; font-weight: 800; color: #0f172a; margin: 0 0 16px 0;"">Restablece tu contraseña</h2>
        <p style=""font-size: 14px; line-height: 22px; color: #334155; margin: 0 0 16px 0;"">
          Hola, <strong>{WebUtility.HtmlEncode(nombre)}</strong>:
        </p>
        <p style=""font-size: 14px; line-height: 22px; color: #334155; margin: 0 0 24px 0;"">
          Recibimos una solicitud para restablecer la contraseña de tu cuenta en AutoStock. Para continuar y crear una nueva contraseña, presiona el siguiente botón:
        </p>
        <div style=""text-align: center; margin: 28px 0;"">
          <a href=""{enlace}"" style=""display: inline-block; background-color: #2563eb; color: #ffffff; font-weight: 800; font-size: 14px; text-decoration: none; padding: 14px 28px; border-radius: 12px; box-shadow: 0 2px 4px rgba(37,99,235,0.25);"">
            Restablecer mi contraseña
          </a>
        </div>
        <p style=""font-size: 13px; line-height: 20px; color: #64748b; margin: 24px 0 8px 0;"">
          O copia y pega el siguiente enlace directamente en tu navegador:
        </p>
        <p style=""font-size: 12px; word-break: break-all; background-color: #f1f5f9; padding: 12px; border-radius: 8px; color: #2563eb; margin: 0 0 24px 0;"">
          <a href=""{enlace}"" style=""color: #2563eb; text-decoration: underline;"">{enlace}</a>
        </p>
        <div style=""border-top: 1px solid #e2e8f0; padding-top: 20px; margin-top: 24px;"">
          <p style=""font-size: 12px; line-height: 18px; color: #64748b; margin: 0 0 8px 0;"">
            <strong>Vigencia:</strong> Este enlace es de un solo uso y expirará en <strong>30 minutos</strong>.
          </p>
          <p style=""font-size: 12px; line-height: 18px; color: #64748b; margin: 0;"">
            Si no solicitaste este cambio, puedes ignorar este correo; tu cuenta y contraseña actual permanecen seguras.
          </p>
        </div>
      </td>
    </tr>
    <tr>
      <td style=""background-color: #f8fafc; padding: 20px 32px; text-align: center; border-top: 1px solid #e2e8f0;"">
        <p style=""font-size: 11px; color: #94a3b8; margin: 0;"">
          © {DateTime.UtcNow.Year} AutoStock. Todos los derechos reservados.
        </p>
      </td>
    </tr>
  </table>
</body>
</html>";
    }
}
