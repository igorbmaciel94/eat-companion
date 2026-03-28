using EatCompanion.Application.Common;
using EatCompanion.Application.DTOs;
using EatCompanion.Application.Interfaces;

namespace EatCompanion.Application.UseCases.MealPlans;

public record GetImportStatusQuery(Guid JobId, Guid UserId);

public class GetImportStatusQueryHandler
{
    private readonly IImportJobRepository _importJobRepo;

    public GetImportStatusQueryHandler(IImportJobRepository importJobRepo)
    {
        _importJobRepo = importJobRepo;
    }

    public async Task<ImportJobStatusDto> Handle(GetImportStatusQuery query)
    {
        var job = await _importJobRepo.GetByIdAsync(query.JobId);
        if (job is null || job.UserId != query.UserId)
            throw new NotFoundException("Import job not found.");

        return new ImportJobStatusDto(job.Id, job.Status, job.MealPlanId, job.ErrorMessage);
    }
}
