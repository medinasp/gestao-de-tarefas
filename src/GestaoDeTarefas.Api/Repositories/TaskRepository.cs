using GestaoDeTarefas.Api.Domain;
using GestaoDeTarefas.Api.Infrastructure;
using Microsoft.EntityFrameworkCore;

namespace GestaoDeTarefas.Api.Repositories;

public sealed class TaskRepository(AppDbContext db) : ITaskRepository
{
    public Task<TodoTask?> GetByCodeAsync(string code, CancellationToken cancellationToken = default) =>
        db.Tasks.FirstOrDefaultAsync(t => t.Code == code, cancellationToken);

    public async Task<IReadOnlyList<TodoTask>> ListAsync(TodoTaskStatus? status, DateOnly? dueDate, CancellationToken cancellationToken = default) =>
        await db.Tasks
            .AsNoTracking()
            .Where(t => (status == null || t.Status == status) && (dueDate == null || t.DueDate == dueDate))
            .OrderBy(t => t.CreatedAt)
            .ToListAsync(cancellationToken);

    public async Task AddAsync(TodoTask task, CancellationToken cancellationToken = default)
    {
        db.Tasks.Add(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(TodoTask task, CancellationToken cancellationToken = default)
    {
        db.Tasks.Update(task);
        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(TodoTask task, CancellationToken cancellationToken = default)
    {
        db.Tasks.Remove(task);
        await db.SaveChangesAsync(cancellationToken);
    }
}
