using FluentValidation;
using GestaoDeTarefas.Api.Dtos;

namespace GestaoDeTarefas.Api.Validation;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be one of: Pending, InProgress, Done.");
    }
}
