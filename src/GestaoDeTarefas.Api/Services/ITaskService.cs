using GestaoDeTarefas.Api.Domain;
using GestaoDeTarefas.Api.Dtos;

namespace GestaoDeTarefas.Api.Services;

public interface ITaskService
{
    Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TaskResponse>> ListAsync(TodoTaskStatus? status, DateOnly? dueDate, string? search, CancellationToken cancellationToken = default);

    Task<TaskResponse> GetAsync(string code, CancellationToken cancellationToken = default);

    Task<TaskResponse> UpdateAsync(string code, UpdateTaskRequest request, CancellationToken cancellationToken = default);

    Task<TaskResponse> PatchAsync(string code, PatchTaskRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(string code, CancellationToken cancellationToken = default);
}
