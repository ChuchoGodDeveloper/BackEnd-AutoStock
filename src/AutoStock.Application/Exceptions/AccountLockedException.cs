namespace AutoStock.Application.Exceptions;

public class AccountLockedException : Exception
{
    public AccountLockedException()
    {
    }

    public AccountLockedException(string message)
        : base(message)
    {
    }

    public AccountLockedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
