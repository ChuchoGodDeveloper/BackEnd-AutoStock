namespace AutoStock.Application.Exceptions;

public class AccountDisabledException : Exception
{
    public AccountDisabledException()
    {
    }

    public AccountDisabledException(string message)
        : base(message)
    {
    }

    public AccountDisabledException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
