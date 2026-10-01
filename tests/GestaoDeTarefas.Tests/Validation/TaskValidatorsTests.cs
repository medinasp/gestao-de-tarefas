using FluentValidation.TestHelper;
using GestaoDeTarefas.Api.Domain;
using GestaoDeTarefas.Api.Dtos;
using GestaoDeTarefas.Api.Validation;

namespace GestaoDeTarefas.Tests.Validation;

public class TaskValidatorsTests
{
    private readonly CreateTaskRequestValidator _create = new();
    private readonly UpdateTaskRequestValidator _update = new();
    private readonly PatchTaskRequestValidator _patch = new();

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
    public void Update_requires_status()
    {
        var request = new UpdateTaskRequest("Valid", null, null, null);

        var result = _update.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Status);
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

    [Fact]
    public void Patch_rejects_empty_request()
    {
        var request = new PatchTaskRequest(null, null, null, null);

        var result = _patch.TestValidate(request);

        Assert.False(result.IsValid);
    }

    [Fact]
    public void Patch_rejects_empty_title_when_provided()
    {
        var request = new PatchTaskRequest("", null, null, null);

        var result = _patch.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Patch_accepts_single_field()
    {
        var request = new PatchTaskRequest(null, "só a descrição", null, null);

        var result = _patch.TestValidate(request);

        result.ShouldNotHaveAnyValidationErrors();
    }
}
