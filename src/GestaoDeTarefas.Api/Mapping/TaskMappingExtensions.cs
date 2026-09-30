using GestaoDeTarefas.Api.Domain;
using GestaoDeTarefas.Api.Dtos;

namespace GestaoDeTarefas.Api.Mapping;

public static class TaskMappingExtensions
{
    public static TaskResponse ToResponse(this TodoTask task) => new()
    {
        Code = task.Code,
        Title = task.Title,
        Description = task.Description,
        DueDate = task.DueDate,
        Status = task.Status,
        CreatedAt = task.CreatedAt
    };
}
