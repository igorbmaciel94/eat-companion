using System.Threading.Channels;
using EatCompanion.Application.Common;
using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;

namespace EatCompanion.Application.UseCases.MealPlans;

public record EnqueueImportCommand(Stream FileStream, string FileName, Guid UserId);

public class EnqueueImportCommandHandler
{
    private readonly IImportJobRepository _importJobRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly Channel<Guid> _importChannel;

    public EnqueueImportCommandHandler(
        IImportJobRepository importJobRepo,
        IUnitOfWork unitOfWork,
        Channel<Guid> importChannel)
    {
        _importJobRepo = importJobRepo;
        _unitOfWork = unitOfWork;
        _importChannel = importChannel;
    }

    public async Task<Guid> Handle(EnqueueImportCommand command)
    {
        var pending = await _importJobRepo.GetPendingByUserAsync(command.UserId);
        if (pending is not null)
            throw new ConflictException("An import is already in progress.", pending.Id);

        var tempDir = Path.Combine(Path.GetTempPath(), "eatcompanion-imports");
        Directory.CreateDirectory(tempDir);
        var tempPath = Path.Combine(tempDir, $"{Guid.NewGuid()}.pdf");
        await using (var fs = File.Create(tempPath))
        {
            await command.FileStream.CopyToAsync(fs);
        }

        var job = new ImportJob
        {
            Id = Guid.NewGuid(),
            UserId = command.UserId,
            FileName = command.FileName,
            TempFilePath = tempPath,
            Status = ImportJobStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _importJobRepo.AddAsync(job);
        await _unitOfWork.SaveChangesAsync();

        await _importChannel.Writer.WriteAsync(job.Id);

        return job.Id;
    }
}
