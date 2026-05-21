namespace BlogSharp.Api.Exceptions;

public class AcessoNegadoException : Exception
{
    public AcessoNegadoException(string message)
        : base(message)
    {
    }
}
