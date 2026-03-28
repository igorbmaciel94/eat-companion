using EatCompanion.Application.Common;
using EatCompanion.Application.Interfaces;
using EatCompanion.Application.UseCases.WeightEntries;
using EatCompanion.Domain.Entities;

namespace EatCompanion.Infrastructure.Tests;

public class WeightEntryUpsertTests
{
    private class FakeWeightEntryRepository : IWeightEntryRepository
    {
        public List<WeightEntry> Entries { get; } = new();

        public Task<List<WeightEntry>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            var result = Entries
                .Where(e => e.UserId == userId && e.Date >= startDate && e.Date <= endDate)
                .OrderBy(e => e.Date)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<WeightEntry?> GetByIdAsync(Guid id)
        {
            return Task.FromResult(Entries.FirstOrDefault(e => e.Id == id));
        }

        public Task<WeightEntry?> GetByUserAndDateAsync(Guid userId, DateOnly date)
        {
            return Task.FromResult(Entries.FirstOrDefault(e => e.UserId == userId && e.Date == date));
        }

        public Task AddAsync(WeightEntry entry)
        {
            Entries.Add(entry);
            return Task.CompletedTask;
        }

        public void Update(WeightEntry entry)
        {
            // In-memory: entry is already modified by reference
        }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(1);
        }
    }

    [Fact]
    public async Task AddWeightEntry_CreatesNewEntry_WhenNoExistingEntryForDate()
    {
        var repo = new FakeWeightEntryRepository();
        var uow = new FakeUnitOfWork();
        var handler = new AddWeightEntryCommandHandler(repo, uow);

        var userId = Guid.NewGuid();
        var command = new AddWeightEntryCommand(userId, new DateOnly(2026, 3, 27), 75.5m, null);

        var result = await handler.Handle(command);

        Assert.Single(repo.Entries);
        Assert.Equal(75.5m, result.WeightKg);
        Assert.Equal(new DateOnly(2026, 3, 27), result.Date);
    }

    [Fact]
    public async Task AddWeightEntry_UpdatesExisting_WhenEntryForSameDateExists()
    {
        var repo = new FakeWeightEntryRepository();
        var uow = new FakeUnitOfWork();
        var handler = new AddWeightEntryCommandHandler(repo, uow);

        var userId = Guid.NewGuid();
        var date = new DateOnly(2026, 3, 27);

        // Add first entry
        var firstCommand = new AddWeightEntryCommand(userId, date, 75.5m, null);
        await handler.Handle(firstCommand);

        // Add second entry for same date — should upsert
        var secondCommand = new AddWeightEntryCommand(userId, date, 74.8m, "corrected");
        var result = await handler.Handle(secondCommand);

        // Should still be one entry, not two
        Assert.Single(repo.Entries);
        Assert.Equal(74.8m, result.WeightKg);
        Assert.Equal(74.8m, repo.Entries[0].WeightKg);
        Assert.Equal("corrected", repo.Entries[0].Notes);
    }

    [Fact]
    public async Task AddWeightEntry_CreatesSeparateEntries_ForDifferentDates()
    {
        var repo = new FakeWeightEntryRepository();
        var uow = new FakeUnitOfWork();
        var handler = new AddWeightEntryCommandHandler(repo, uow);

        var userId = Guid.NewGuid();

        await handler.Handle(new AddWeightEntryCommand(userId, new DateOnly(2026, 3, 26), 75.5m, null));
        await handler.Handle(new AddWeightEntryCommand(userId, new DateOnly(2026, 3, 27), 75.0m, null));

        Assert.Equal(2, repo.Entries.Count);
    }

    [Fact]
    public async Task UpdateWeightEntry_ReturnsNull_WhenEntryNotFound()
    {
        var repo = new FakeWeightEntryRepository();
        var uow = new FakeUnitOfWork();
        var handler = new UpdateWeightEntryCommandHandler(repo, uow);

        var command = new UpdateWeightEntryCommand(Guid.NewGuid(), Guid.NewGuid(), 75m, null);
        var result = await handler.Handle(command);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateWeightEntry_ReturnsNull_WhenEntryBelongsToDifferentUser()
    {
        var repo = new FakeWeightEntryRepository();
        var uow = new FakeUnitOfWork();

        var entry = new WeightEntry
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            Date = new DateOnly(2026, 3, 27),
            WeightKg = 75m,
            CreatedAt = DateTime.UtcNow
        };
        repo.Entries.Add(entry);

        var handler = new UpdateWeightEntryCommandHandler(repo, uow);
        var command = new UpdateWeightEntryCommand(Guid.NewGuid(), entry.Id, 74m, null);
        var result = await handler.Handle(command);

        Assert.Null(result);
        Assert.Equal(75m, entry.WeightKg); // unchanged
    }

    [Fact]
    public async Task UpdateWeightEntry_UpdatesEntry_WhenValid()
    {
        var repo = new FakeWeightEntryRepository();
        var uow = new FakeUnitOfWork();

        var userId = Guid.NewGuid();
        var entry = new WeightEntry
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = new DateOnly(2026, 3, 27),
            WeightKg = 75m,
            CreatedAt = DateTime.UtcNow
        };
        repo.Entries.Add(entry);

        var handler = new UpdateWeightEntryCommandHandler(repo, uow);
        var command = new UpdateWeightEntryCommand(userId, entry.Id, 74.2m, "after workout");
        var result = await handler.Handle(command);

        Assert.NotNull(result);
        Assert.Equal(74.2m, result.WeightKg);
        Assert.Equal(74.2m, entry.WeightKg);
        Assert.Equal("after workout", entry.Notes);
    }
}
