using GestaoDeTarefas.Api.Domain;
using GestaoDeTarefas.Api.Dtos;
using GestaoDeTarefas.Api.Repositories;
using GestaoDeTarefas.Api.Services;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;

namespace GestaoDeTarefas.Tests.Services;

public class TaskServiceTests
{
    private readonly ITaskRepository _repository = Substitute.For<ITaskRepository>();
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _service = new TaskService(_repository, NullLogger<TaskService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_defaults_status_to_Pending_when_not_provided()
    {
        var request = new CreateTaskRequest("Task", null, null, null);

        var result = await _service.CreateAsync(request);

        Assert.Equal(TodoTaskStatus.Pending, result.Status);
        await _repository.Received(1).AddAsync(Arg.Any<TodoTask>(), Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task CreateAsync_uses_provided_status_and_returns_generated_code()
    {
        var request = new CreateTaskRequest("Task", "desc", null, TodoTaskStatus.InProgress);

        var result = await _service.CreateAsync(request);

        Assert.Equal(TodoTaskStatus.InProgress, result.Status);
        Assert.StartsWith("TSK-", result.Code);
        Assert.Equal("Task", result.Title);
    }

    [Fact]
    public async Task GetAsync_throws_NotFound_when_missing()
    {
        _repository.GetByCodeAsync("X", Arg.Any<CancellationToken>()).Returns((TodoTask?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.GetAsync("X"));
    }

    [Fact]
    public async Task GetAsync_returns_mapped_task()
    {
        var task = TodoTask.Create("Task", "desc", null, TodoTaskStatus.Pending);
        _repository.GetByCodeAsync(task.Code, Arg.Any<CancellationToken>()).Returns(task);

        var result = await _service.GetAsync(task.Code);

        Assert.Equal(task.Code, result.Code);
        Assert.Equal("Task", result.Title);
    }

    [Fact]
    public async Task UpdateAsync_throws_NotFound_when_missing()
    {
        _repository.GetByCodeAsync("X", Arg.Any<CancellationToken>()).Returns((TodoTask?)null);
        var request = new UpdateTaskRequest("New", null, null, TodoTaskStatus.Done);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.UpdateAsync("X", request));
    }

    [Fact]
    public async Task UpdateAsync_applies_changes_and_persists()
    {
        var task = TodoTask.Create("Old", null, null, TodoTaskStatus.Pending);
        _repository.GetByCodeAsync(task.Code, Arg.Any<CancellationToken>()).Returns(task);
        var request = new UpdateTaskRequest("New", "new desc", new DateOnly(2027, 5, 1), TodoTaskStatus.Done);

        var result = await _service.UpdateAsync(task.Code, request);

        Assert.Equal("New", result.Title);
        Assert.Equal(TodoTaskStatus.Done, result.Status);
        await _repository.Received(1).UpdateAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task DeleteAsync_throws_NotFound_when_missing()
    {
        _repository.GetByCodeAsync("X", Arg.Any<CancellationToken>()).Returns((TodoTask?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _service.DeleteAsync("X"));
    }

    [Fact]
    public async Task DeleteAsync_removes_existing_task()
    {
        var task = TodoTask.Create("Task", null, null, TodoTaskStatus.Pending);
        _repository.GetByCodeAsync(task.Code, Arg.Any<CancellationToken>()).Returns(task);

        await _service.DeleteAsync(task.Code);

        await _repository.Received(1).DeleteAsync(task, Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task ListAsync_forwards_filters_and_maps_results()
    {
        var tasks = new List<TodoTask>
        {
            TodoTask.Create("A", null, null, TodoTaskStatus.Pending),
            TodoTask.Create("B", null, null, TodoTaskStatus.Pending)
        };
        _repository.ListAsync(TodoTaskStatus.Pending, null, Arg.Any<CancellationToken>()).Returns(tasks);

        var result = await _service.ListAsync(TodoTaskStatus.Pending, null);

        Assert.Equal(2, result.Count);
        await _repository.Received(1).ListAsync(TodoTaskStatus.Pending, null, Arg.Any<CancellationToken>());
    }
}
