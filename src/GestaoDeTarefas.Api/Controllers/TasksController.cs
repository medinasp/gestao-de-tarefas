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
    /// <summary>Cria uma nova tarefa.</summary>
    /// <response code="201">Tarefa criada; retorna o recurso com o código gerado e os links.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    [HttpPost]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create(CreateTaskRequest request, CancellationToken cancellationToken)
    {
        await createValidator.ValidateAndThrowAsync(request, cancellationToken);

        var task = WithLinks(await service.CreateAsync(request, cancellationToken));

        return CreatedAtAction(nameof(GetByCode), new { code = task.Code }, task);
    }

    /// <summary>Lista as tarefas, com filtros opcionais por status e/ou data de vencimento.</summary>
    /// <response code="200">Lista de tarefas.</response>
    /// <response code="400">Filtro inválido (por exemplo, um status inexistente).</response>
    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TaskResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> List(
        [FromQuery] TodoTaskStatus? status,
        [FromQuery] DateOnly? dueDate,
        CancellationToken cancellationToken)
    {
        var tasks = (await service.ListAsync(status, dueDate, cancellationToken))
            .Select(WithLinks)
            .ToList();

        return Ok(tasks);
    }

    /// <summary>Obtém uma tarefa pelo código.</summary>
    /// <response code="200">Tarefa encontrada.</response>
    /// <response code="404">Nenhuma tarefa com o código informado.</response>
    [HttpGet("{code}", Name = nameof(GetByCode))]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, CancellationToken cancellationToken)
    {
        return Ok(WithLinks(await service.GetAsync(code, cancellationToken)));
    }

    /// <summary>Atualiza uma tarefa existente (substituição completa).</summary>
    /// <response code="200">Tarefa atualizada.</response>
    /// <response code="400">Dados de entrada inválidos.</response>
    /// <response code="404">Nenhuma tarefa com o código informado.</response>
    [HttpPut("{code}")]
    [ProducesResponseType(typeof(TaskResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(string code, UpdateTaskRequest request, CancellationToken cancellationToken)
    {
        await updateValidator.ValidateAndThrowAsync(request, cancellationToken);

        return Ok(WithLinks(await service.UpdateAsync(code, request, cancellationToken)));
    }

    /// <summary>Remove uma tarefa.</summary>
    /// <response code="204">Tarefa removida.</response>
    /// <response code="404">Nenhuma tarefa com o código informado.</response>
    [HttpDelete("{code}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(string code, CancellationToken cancellationToken)
    {
        await service.DeleteAsync(code, cancellationToken);
        return NoContent();
    }

    private static TaskResponse WithLinks(TaskResponse task)
    {
        var self = $"/api/tasks/{task.Code}";
        task.Links = new Dictionary<string, LinkDto>
        {
            ["self"] = new(self, "GET"),
            ["update"] = new(self, "PUT"),
            ["delete"] = new(self, "DELETE")
        };
        return task;
    }
}
