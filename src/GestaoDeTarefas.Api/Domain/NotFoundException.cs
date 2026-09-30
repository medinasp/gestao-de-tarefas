namespace GestaoDeTarefas.Api.Domain;

public sealed class NotFoundException(string message) : Exception(message)
{
    public static NotFoundException Task(string code) =>
        new($"Task '{code}' was not found.");
}
