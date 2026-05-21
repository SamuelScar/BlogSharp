namespace BlogSharp.Api.Exceptions;

public class RequisicaoInvalidaException : Exception
{
    public RequisicaoInvalidaException(string message)
        : base(message)
    {
    }
}
