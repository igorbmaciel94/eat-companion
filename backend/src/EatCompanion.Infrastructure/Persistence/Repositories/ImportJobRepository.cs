using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;
using EatCompanion.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace EatCompanion.Infrastructure.Persistence.Repositories;

public class ImportJobRepository : IImportJobRepository
{
    private readonly AppDbContext _context;

    public ImportJobRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<ImportJob?> GetByIdAsync(Guid id)
    {
        return await _context.ImportJobs.FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<ImportJob?> GetPendingByUserAsync(Guid userId)
    {
        return await _context.ImportJobs
            .FirstOrDefaultAsync(j => j.UserId == userId &&
                (j.Status == ImportJobStatus.Pending || j.Status == ImportJobStatus.Processing));
    }

    public async Task AddAsync(ImportJob job)
    {
        await _context.ImportJobs.AddAsync(job);
    }

    public void Update(ImportJob job)
    {
        _context.ImportJobs.Update(job);
    }
}
