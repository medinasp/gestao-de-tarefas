namespace GestaoDeTarefas.Api.Domain;

public class TodoTask
{
    public string Code { get; private set; } = null!;
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateOnly? DueDate { get; private set; }
    public TodoTaskStatus Status { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private TodoTask(string code, string title, string? description, DateOnly? dueDate, TodoTaskStatus status, DateTimeOffset createdAt)
    {
        Code = code;
        Title = title;
        Description = description;
        DueDate = dueDate;
        Status = status;
        CreatedAt = createdAt;
    }

    public static TodoTask Create(string title, string? description, DateOnly? dueDate, TodoTaskStatus status)
    {
        EnsureTitle(title);
        return new TodoTask(GenerateCode(), title.Trim(), Normalize(description), dueDate, status, DateTimeOffset.UtcNow);
    }

    public void Update(string title, string? description, DateOnly? dueDate, TodoTaskStatus status)
    {
        EnsureTitle(title);
        Title = title.Trim();
        Description = Normalize(description);
        DueDate = dueDate;
        Status = status;
    }

    private static void EnsureTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Title is required.", nameof(title));
        }
    }

    private static string? Normalize(string? value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private static string GenerateCode() =>
        "TSK-" + Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
}
