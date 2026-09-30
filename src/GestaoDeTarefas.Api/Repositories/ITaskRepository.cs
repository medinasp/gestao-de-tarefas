using GestaoDeTarefas.Api.Domain;

namespace GestaoDeTarefas.Api.Repositories;

public interface ITaskRepository
{
    Task<TodoTask?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TodoTask>> ListAsync(TodoTaskStatus? status, DateOnly? dueDate, string? search, CancellationToken cancellationToken = default);

    Task AddAsync(TodoTask task, CancellationToken cancellationToken = default);

    Task UpdateAsync(TodoTask task, CancellationToken cancellationToken = default);

    Task DeleteAsync(TodoTask task, CancellationToken cancellationToken = default);
}
