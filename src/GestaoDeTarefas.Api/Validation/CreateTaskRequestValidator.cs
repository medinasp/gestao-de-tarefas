using FluentValidation;
using GestaoDeTarefas.Api.Dtos;

namespace GestaoDeTarefas.Api.Validation;

public sealed class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000);

        RuleFor(x => x.Status)
            .IsInEnum().When(x => x.Status is not null)
            .WithMessage("Status must be one of: Pending, InProgress, Done.");
    }
}
