using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;

namespace EatCompanion.Infrastructure.Services;

/// <summary>
/// Stub implementation. Will be replaced by Track 5.
/// </summary>
public class StubPdfParsingService : IPdfParsingService
{
    public Task<MealPlan> ParseAsync(Stream pdfStream, string fileName, Guid userId)
    {
        throw new NotImplementedException("PDF parsing service is not yet implemented. This will be provided by Track 5.");
    }
}
