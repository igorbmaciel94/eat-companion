using EatCompanion.Application.UseCases.Profile;
using FluentValidation;

namespace EatCompanion.Application.Validators;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name cannot be empty.")
            .When(x => x.DisplayName is not null);

        RuleFor(x => x.CalorieTarget)
            .GreaterThan(0).WithMessage("Calorie target must be greater than 0.")
            .When(x => x.CalorieTarget is not null);

        RuleFor(x => x.GoalType)
            .IsInEnum().WithMessage("A valid goal type is required.")
            .When(x => x.GoalType is not null);
    }
}
