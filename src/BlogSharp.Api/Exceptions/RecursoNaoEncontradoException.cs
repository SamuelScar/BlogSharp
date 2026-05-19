namespace BlogSharp.Api.Exceptions;

public class RecursoNaoEncontradoException : Exception
{
    public RecursoNaoEncontradoException(string message)
        : base(message)
    {
    }
}
