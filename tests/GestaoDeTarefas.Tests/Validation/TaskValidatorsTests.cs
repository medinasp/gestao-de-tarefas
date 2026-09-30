using FluentValidation.TestHelper;
using GestaoDeTarefas.Api.Domain;
using GestaoDeTarefas.Api.Dtos;
using GestaoDeTarefas.Api.Validation;

namespace GestaoDeTarefas.Tests.Validation;

public class TaskValidatorsTests
{
    private readonly CreateTaskRequestValidator _create = new();
    private readonly UpdateTaskRequestValidator _update = new();

    [Fact]
    public void Create_requires_title()
    {
        var result = _create.TestValidate(new CreateTaskRequest("", null, null, null));

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Create_rejects_title_longer_than_200()
    {
        var request = new CreateTaskRequest(new string('a', 201), null, null, null);

        var result = _create.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Create_accepts_valid_request()
    {
        var request = new CreateTaskRequest("Valid", "desc", new DateOnly(2026, 10, 1), TodoTaskStatus.Pending);

        var result = _create.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Update_rejects_undefined_status()
    {
        var request = new UpdateTaskRequest("Valid", null, null, (TodoTaskStatus)999);

        var result = _update.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Update_accepts_valid_request()
    {
        var request = new UpdateTaskRequest("Valid", null, null, TodoTaskStatus.Done);

        var result = _update.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
