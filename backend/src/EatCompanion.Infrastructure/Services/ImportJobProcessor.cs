using System.Threading.Channels;
using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Enums;
using EatCompanion.Domain.Interfaces;
using EatCompanion.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EatCompanion.Infrastructure.Services;

public class ImportJobProcessor : BackgroundService
{
    private readonly Channel<Guid> _channel;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ImportJobProcessor> _logger;

    public ImportJobProcessor(
        Channel<Guid> channel,
        IServiceScopeFactory scopeFactory,
        ILogger<ImportJobProcessor> logger)
    {
        _channel = channel;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await foreach (var jobId in _channel.Reader.ReadAllAsync(stoppingToken))
        {
            try
            {
                await ProcessJobAsync(jobId, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unhandled error processing import job {JobId}", jobId);
            }
        }
    }

    private async Task ProcessJobAsync(Guid jobId, CancellationToken ct)
    {
        using var scope = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var pdfParser = scope.ServiceProvider.GetRequiredService<IPdfParsingService>();
        var mealPlanRepo = scope.ServiceProvider.GetRequiredService<IMealPlanRepository>();

        var job = await context.ImportJobs.FirstOrDefaultAsync(j => j.Id == jobId, ct);
        if (job is null) return;

        job.Status = ImportJobStatus.Processing;
        await context.SaveChangesAsync(ct);

        try
        {
            await using var stream = File.OpenRead(job.TempFilePath);
            var mealPlan = await pdfParser.ParseAsync(stream, job.FileName, job.UserId);

            // Deactivate existing plans
            await mealPlanRepo.DeactivateAllForUserAsync(job.UserId);
            await mealPlanRepo.AddAsync(mealPlan);
            await context.SaveChangesAsync(ct);

            job.Status = ImportJobStatus.Completed;
            job.MealPlanId = mealPlan.Id;
            job.CompletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);

            _logger.LogInformation("Import job {JobId} completed. MealPlanId: {MealPlanId}", jobId, mealPlan.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Import job {JobId} failed", jobId);
            job.Status = ImportJobStatus.Failed;
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;
            await context.SaveChangesAsync(ct);
        }
        finally
        {
            // Cleanup temp file
            try { if (File.Exists(job.TempFilePath)) File.Delete(job.TempFilePath); }
            catch { /* ignore cleanup errors */ }
        }
    }
}
