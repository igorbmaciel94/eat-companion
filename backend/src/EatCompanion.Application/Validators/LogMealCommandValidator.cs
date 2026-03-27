using EatCompanion.Application.UseCases.MealLogs;
using FluentValidation;

namespace EatCompanion.Application.Validators;

public class LogMealCommandValidator : AbstractValidator<LogMealCommand>
{
    public LogMealCommandValidator()
    {
        RuleFor(x => x.Date)
            .NotEmpty().WithMessage("Date is required.");

        RuleFor(x => x.MealType)
            .IsInEnum().WithMessage("A valid meal type is required.");

        RuleFor(x => x.Status)
            .IsInEnum().WithMessage("A valid status is required.");
    }
}
