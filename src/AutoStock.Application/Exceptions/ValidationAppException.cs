namespace AutoStock.Application.Exceptions;

public class ValidationAppException : Exception
{
    public ValidationAppException()
    {
    }

    public ValidationAppException(string message)
        : base(message)
    {
    }

    public ValidationAppException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
