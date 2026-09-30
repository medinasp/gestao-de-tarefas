using GestaoDeTarefas.Api.Domain;

namespace GestaoDeTarefas.Api.Dtos;

public sealed record UpdateTaskRequest(
    string Title,
    string? Description,
    DateOnly? DueDate,
    TodoTaskStatus? Status);
