using FluentValidation;

namespace DevStack.Api.GraphQL.Types;

public class CreateDeliverableInputValidator : AbstractValidator<CreateDeliverableInput>
{
    public CreateDeliverableInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required");

        RuleFor(x => x.Type)
            .NotEmpty().WithMessage("Type is required");

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required");
    }
}

public class UpdateDeliverableInputValidator : AbstractValidator<UpdateDeliverableInput>
{
    public UpdateDeliverableInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
