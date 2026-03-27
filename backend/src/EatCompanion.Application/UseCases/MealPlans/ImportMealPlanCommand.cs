using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Interfaces;

namespace EatCompanion.Application.UseCases.MealPlans;

public record ImportMealPlanCommand(Stream PdfStream, string FileName, Guid UserId);

public class ImportMealPlanCommandHandler
{
    private readonly IPdfParsingService _pdfParser;
    private readonly IMealPlanRepository _mealPlanRepo;
    private readonly IUnitOfWork _unitOfWork;

    public ImportMealPlanCommandHandler(
        IPdfParsingService pdfParser,
        IMealPlanRepository mealPlanRepo,
        IUnitOfWork unitOfWork)
    {
        _pdfParser = pdfParser;
        _mealPlanRepo = mealPlanRepo;
        _unitOfWork = unitOfWork;
    }

    public async Task<ImportResultDto> Handle(ImportMealPlanCommand command, CancellationToken cancellationToken = default)
    {
        var mealPlan = await _pdfParser.ParseAsync(command.PdfStream, command.FileName, command.UserId);

        // Deactivate existing plans before adding the new one
        await _mealPlanRepo.DeactivateAllForUserAsync(command.UserId);
        await _mealPlanRepo.AddAsync(mealPlan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return new ImportResultDto(
            mealPlan.Id,
            mealPlan.Name,
            mealPlan.Days.Count,
            mealPlan.Days.SelectMany(d => d.Meals).Count(),
            mealPlan.Days.SelectMany(d => d.Meals).SelectMany(m => m.Options).Count()
        );
    }
}
