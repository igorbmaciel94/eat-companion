using EatCompanion.Application.UseCases.Profile;
using FluentValidation;

namespace EatCompanion.Application.Validators;

public class UpdateProfileCommandValidator : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.");

        RuleFor(x => x.CalorieTarget)
            .GreaterThan(0).WithMessage("Calorie target must be greater than 0.");

        RuleFor(x => x.GoalType)
            .IsInEnum().WithMessage("A valid goal type is required.");
    }
}
