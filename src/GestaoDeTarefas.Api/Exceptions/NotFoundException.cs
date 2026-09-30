namespace GestaoDeTarefas.Api.Exceptions;

public sealed class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException ForTask(string code) =>
        new($"Task '{code}' was not found.");
}
