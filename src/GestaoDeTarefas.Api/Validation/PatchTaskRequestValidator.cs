using FluentValidation;
using GestaoDeTarefas.Api.Dtos;

namespace GestaoDeTarefas.Api.Validation;

public sealed class PatchTaskRequestValidator : AbstractValidator<PatchTaskRequest>
{
    public PatchTaskRequestValidator()
    {
        RuleFor(x => x)
            .Must(HaveAtLeastOneField)
            .WithMessage("Provide at least one field to update.");

        When(x => x.Title is not null, () =>
            RuleFor(x => x.Title)
                .NotEmpty().WithMessage("Title cannot be empty.")
                .MaximumLength(200));

        When(x => x.Description is not null, () =>
            RuleFor(x => x.Description).MaximumLength(1000));

        When(x => x.Status is not null, () =>
            RuleFor(x => x.Status)
                .IsInEnum().WithMessage("Status must be one of: Pending, InProgress, Done."));
    }

    private static bool HaveAtLeastOneField(PatchTaskRequest request) =>
        request.Title is not null
        || request.Description is not null
        || request.DueDate is not null
        || request.Status is not null;
}
