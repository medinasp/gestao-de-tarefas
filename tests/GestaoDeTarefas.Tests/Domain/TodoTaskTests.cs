using GestaoDeTarefas.Api.Domain;

namespace GestaoDeTarefas.Tests.Domain;

public class TodoTaskTests
{
    [Fact]
    public void Create_generates_code_with_expected_prefix()
    {
        var task = TodoTask.Create("Buy coffee", null, null, TodoTaskStatus.Pending);

        Assert.StartsWith("TSK-", task.Code);
        Assert.True(task.Code.Length > 4);
    }

    [Fact]
    public void Create_sets_all_fields()
    {
        var due = new DateOnly(2026, 10, 1);

        var task = TodoTask.Create("Write report", "Quarterly", due, TodoTaskStatus.InProgress);

        Assert.Equal("Write report", task.Title);
        Assert.Equal("Quarterly", task.Description);
        Assert.Equal(due, task.DueDate);
        Assert.Equal(TodoTaskStatus.InProgress, task.Status);
        Assert.NotEqual(default, task.CreatedAt);
    }

    [Fact]
    public void Create_trims_title_and_description()
    {
        var task = TodoTask.Create("  spaced  ", "  desc  ", null, TodoTaskStatus.Pending);

        Assert.Equal("spaced", task.Title);
        Assert.Equal("desc", task.Description);
    }

    [Fact]
    public void Create_turns_blank_description_into_null()
    {
        var task = TodoTask.Create("Title", "   ", null, TodoTaskStatus.Pending);

        Assert.Null(task.Description);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_with_missing_title_throws(string? title)
    {
        Assert.Throws<ArgumentException>(() =>
            TodoTask.Create(title!, null, null, TodoTaskStatus.Pending));
    }

    [Fact]
    public void Update_changes_all_editable_fields()
    {
        var task = TodoTask.Create("Old", "old desc", null, TodoTaskStatus.Pending);
        var due = new DateOnly(2027, 1, 15);

        task.Update("New", "new desc", due, TodoTaskStatus.Done);

        Assert.Equal("New", task.Title);
        Assert.Equal("new desc", task.Description);
        Assert.Equal(due, task.DueDate);
        Assert.Equal(TodoTaskStatus.Done, task.Status);
    }

    [Fact]
    public void Update_keeps_code_unchanged()
    {
        var task = TodoTask.Create("Old", null, null, TodoTaskStatus.Pending);
        var originalCode = task.Code;

        task.Update("New", null, null, TodoTaskStatus.Done);

        Assert.Equal(originalCode, task.Code);
    }

    [Fact]
    public void Update_with_missing_title_throws()
    {
        var task = TodoTask.Create("Old", null, null, TodoTaskStatus.Pending);

        Assert.Throws<ArgumentException>(() =>
            task.Update("", null, null, TodoTaskStatus.Done));
    }
}
