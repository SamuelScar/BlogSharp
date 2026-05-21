namespace BlogSharp.Api.Exceptions;

public class IntegracaoIAException : Exception
{
    public IntegracaoIAException(string message)
        : base(message)
    {
    }

    public IntegracaoIAException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
