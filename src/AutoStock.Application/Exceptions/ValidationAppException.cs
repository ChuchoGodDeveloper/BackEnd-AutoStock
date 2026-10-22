namespace AutoStock.Application.Exceptions;

public class ValidationAppException : Exception
{
    public Guid? VehiculoExistenteId { get; }

    public ValidationAppException()
    {
    }

    public ValidationAppException(string message)
        : base(message)
    {
    }

    public ValidationAppException(string message, Guid vehiculoExistenteId)
        : base(message)
    {
        VehiculoExistenteId = vehiculoExistenteId;
    }

    public ValidationAppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
