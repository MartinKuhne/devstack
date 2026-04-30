namespace DevStack.Domain.Validation;

public static class ValidationRules
{
    public static ValidationResult ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return new ValidationResult(false, ["Name is required."]);

        if (name.Length > 200)
            return new ValidationResult(false, ["Name must be 200 characters or less."]);

        return ValidationResult.Valid;
    }

    public static ValidationResult ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return new ValidationResult(false, ["Title is required."]);

        if (title.Length > 200)
            return new ValidationResult(false, ["Title must be 200 characters or less."]);

        return ValidationResult.Valid;
    }
}


