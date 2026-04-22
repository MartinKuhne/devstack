using FluentValidation;

namespace DevStack.Api.GraphQL.Types;

public class CreateAgentTaskInputValidator : AbstractValidator<CreateAgentTaskInput>
{
    public CreateAgentTaskInputValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required");

        RuleFor(x => x.ComplexityRating)
            .InclusiveBetween(1, 10).WithMessage("ComplexityRating must be between 1 and 10");

        RuleFor(x => x.DeliverableId)
            .NotEmpty().WithMessage("DeliverableId is required");
    }
}

public class UpdateAgentTaskInputValidator : AbstractValidator<UpdateAgentTaskInput>
{
    public UpdateAgentTaskInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");

        RuleFor(x => x.ComplexityRating)
            .InclusiveBetween(1, 10).WithMessage("ComplexityRating must be between 1 and 10")
            .When(x => x.ComplexityRating.HasValue);
    }
}
