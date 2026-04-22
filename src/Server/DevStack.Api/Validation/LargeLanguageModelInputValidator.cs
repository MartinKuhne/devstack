using FluentValidation;

namespace DevStack.Api.GraphQL.Types;

public class CreateLargeLanguageModelInputValidator : AbstractValidator<CreateLargeLanguageModelInput>
{
    public CreateLargeLanguageModelInputValidator()
    {
        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("Url is required");

        RuleFor(x => x.Model)
            .NotEmpty().WithMessage("Model is required");

        RuleFor(x => x.ApiKey)
            .NotEmpty().WithMessage("ApiKey is required");

        RuleFor(x => x.MaxComplexity)
            .GreaterThan(0).WithMessage("MaxComplexity must be greater than 0");
    }
}

public class UpdateLargeLanguageModelInputValidator : AbstractValidator<UpdateLargeLanguageModelInput>
{
    public UpdateLargeLanguageModelInputValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required");
    }
}
