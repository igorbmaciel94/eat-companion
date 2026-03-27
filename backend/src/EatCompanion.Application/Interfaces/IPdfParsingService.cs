using EatCompanion.Domain.Entities;

namespace EatCompanion.Application.Interfaces;

public interface IPdfParsingService
{
    Task<MealPlan> ParseAsync(Stream pdfStream, string fileName, Guid userId);
}
