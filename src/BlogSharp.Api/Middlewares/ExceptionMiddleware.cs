using BlogSharp.Api.DTOs;
using BlogSharp.Api.Exceptions;

namespace BlogSharp.Api.Middlewares;

public class ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            if (context.Response.HasStarted)
            {
                throw;
            }

            var (statusCode, mensagem) = MapearErro(exception);

            if (statusCode == StatusCodes.Status500InternalServerError)
            {
                logger.LogError(exception, "Erro inesperado ao processar a requisicao.");
            }

            context.Response.StatusCode = statusCode;

            await context.Response.WriteAsJsonAsync(new ErroResponse(mensagem));
        }
    }

    private static (int StatusCode, string Mensagem) MapearErro(Exception exception)
    {
        return exception switch
        {
            RequisicaoInvalidaException ex => (StatusCodes.Status400BadRequest, ex.Message),
            AcessoNegadoException ex => (StatusCodes.Status403Forbidden, ex.Message),
            RecursoNaoEncontradoException ex => (StatusCodes.Status404NotFound, ex.Message),
            ConflitoException ex => (StatusCodes.Status409Conflict, ex.Message),
            _ => (StatusCodes.Status500InternalServerError, "Erro interno no servidor.")
        };
    }
}
