namespace AutoStock.Application.Exceptions;

public class ForbiddenAppException : Exception
{
    public ForbiddenAppException()
        : base("No tienes permiso para realizar esta acción")
    {
    }

    public ForbiddenAppException(string message)
        : base(message)
    {
    }

    public ForbiddenAppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
