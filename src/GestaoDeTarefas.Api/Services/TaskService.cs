using GestaoDeTarefas.Api.Domain;
using GestaoDeTarefas.Api.Dtos;
using GestaoDeTarefas.Api.Mapping;
using GestaoDeTarefas.Api.Repositories;

namespace GestaoDeTarefas.Api.Services;

public sealed class TaskService(ITaskRepository repository, ILogger<TaskService> logger) : ITaskService
{
    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = TodoTask.Create(
            request.Title,
            request.Description,
            request.DueDate,
            request.Status ?? TodoTaskStatus.Pending);

        await repository.AddAsync(task, cancellationToken);
        logger.LogInformation("Task {Code} created.", task.Code);

        return task.ToResponse();
    }

    public async Task<IReadOnlyList<TaskResponse>> ListAsync(TodoTaskStatus? status, DateOnly? dueDate, CancellationToken cancellationToken = default)
    {
        var tasks = await repository.ListAsync(status, dueDate, cancellationToken);
        return [.. tasks.Select(t => t.ToResponse())];
    }

    public async Task<TaskResponse> GetAsync(string code, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw NotFoundException.Task(code);

        return task.ToResponse();
    }

    public async Task<TaskResponse> UpdateAsync(string code, UpdateTaskRequest request, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw NotFoundException.Task(code);

        task.Update(request.Title, request.Description, request.DueDate, request.Status);

        await repository.UpdateAsync(task, cancellationToken);
        logger.LogInformation("Task {Code} updated.", task.Code);

        return task.ToResponse();
    }

    public async Task DeleteAsync(string code, CancellationToken cancellationToken = default)
    {
        var task = await repository.GetByCodeAsync(code, cancellationToken)
            ?? throw NotFoundException.Task(code);

        await repository.DeleteAsync(task, cancellationToken);
        logger.LogInformation("Task {Code} deleted.", task.Code);
    }
}
