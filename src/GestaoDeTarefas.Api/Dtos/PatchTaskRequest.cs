using GestaoDeTarefas.Api.Domain;

namespace GestaoDeTarefas.Api.Dtos;

public sealed record PatchTaskRequest(
    string? Title,
    string? Description,
    DateOnly? DueDate,
    TodoTaskStatus? Status);
