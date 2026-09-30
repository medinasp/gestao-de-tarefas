using GestaoDeTarefas.Api.Domain;

namespace GestaoDeTarefas.Api.Dtos;

public sealed record CreateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TodoTaskStatus? Status);
