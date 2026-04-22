using FluentValidation;

namespace DevStack.Api.GraphQL.Types;

public class CreateProjectInputValidator : AbstractValidator<CreateProjectInput>
{
    public CreateProjectInputValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required")
            .MaximumLength(200).WithMessage("Name must be 200 characters or less");
    }
}

public class UpdateProjectInputValidator : AbstractValidator<UpdateProjectInput>
{
    public UpdateProjectInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.Name)
            .MaximumLength(200).WithMessage("Name must be 200 characters or less")
            .When(x => !string.IsNullOrWhiteSpace(x.Name));
    }
}
