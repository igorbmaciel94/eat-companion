using EatCompanion.Application.UseCases.WeightEntries;
using FluentValidation;

namespace EatCompanion.Application.Validators;

public class AddWeightEntryCommandValidator : AbstractValidator<AddWeightEntryCommand>
{
    public AddWeightEntryCommandValidator()
    {
        RuleFor(x => x.WeightKg)
            .GreaterThan(0).WithMessage("Weight must be greater than 0.");

        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");
    }
}
