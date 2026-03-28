using EatCompanion.Domain.Entities;

namespace EatCompanion.Application.Interfaces;

public interface IImportJobRepository
{
    Task<ImportJob?> GetByIdAsync(Guid id);
    Task<ImportJob?> GetPendingByUserAsync(Guid userId);
    Task AddAsync(ImportJob job);
    void Update(ImportJob job);
}
