using FluentValidation;
using GestaoDeTarefas.Api.Domain;
using GestaoDeTarefas.Api.Dtos;
using GestaoDeTarefas.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace GestaoDeTarefas.Api.Controllers;

[ApiController]
[Route("api/tasks")]
[Produces("application/json")]
public sealed class TasksController(
    ITaskService service,
    IValidator<CreateTaskRequest> createValidator,
    IValidator<UpdateTaskRequest> updateValidator) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = await service.CreateAsync(request, cancellationToken);
        WithLinks(task);

        return CreatedAtAction(nameof(GetByCode), new { code = task.Code }, task);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] TodoTaskStatus? status,
        [FromQuery] DateOnly? dueDate,
        CancellationToken cancellationToken)
    {
        var tasks = await service.ListAsync(status, dueDate, cancellationToken);

        foreach (var task in tasks)
        {
            WithLinks(task);
        }

        return Ok(tasks);
    }

    [HttpGet("{code}", Name = nameof(GetByCode))]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        var task = await service.GetAsync(code, cancellationToken);
        WithLinks(task);

        return Ok(task);
    }

    [HttpPut("{code}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string code, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = await service.UpdateAsync(code, request, cancellationToken);
        WithLinks(task);

        return Ok(task);
    }

    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string code, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(code, cancellationToken);
        return NoContent();
    }

    private static void WithLinks(TaskResponse task)
    {
        var self = $"/api/tasks/{task.Code}";
        task.Links = new Dictionary<string, LinkDto>
        {
            ["self"] = new(self, "GET"),
            ["update"] = new(self, "PUT"),
            ["delete"] = new(self, "DELETE")
        };
    }
}
