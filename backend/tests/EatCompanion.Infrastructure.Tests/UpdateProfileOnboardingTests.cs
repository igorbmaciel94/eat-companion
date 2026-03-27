using EatCompanion.Application.Common;
using EatCompanion.Application.UseCases.Profile;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;
using EatCompanion.Domain.Interfaces;
#pragma warning disable CS1998

namespace EatCompanion.Infrastructure.Tests;

public class UpdateProfileOnboardingTests
{
    private class FakeUserRepository : IUserRepository
    {
        private readonly Dictionary<Guid, User> _users = new();

        public void Add(User user) => _users[user.Id] = user;

        public Task<User?> GetByIdAsync(Guid id) =>
            Task.FromResult(_users.GetValueOrDefault(id));

        public Task<User?> GetByEmailAsync(string email) =>
            Task.FromResult(_users.Values.FirstOrDefault(u => u.Email == email));

        public Task<bool> ExistsByEmailAsync(string email) =>
            Task.FromResult(_users.Values.Any(u => u.Email == email));

        public Task AddAsync(User user)
        {
            _users[user.Id] = user;
            return Task.CompletedTask;
        }

        public void Update(User user)
        {
            _users[user.Id] = user;
        }

        public async Task<(User? User, RefreshToken? Token)> GetByRefreshTokenAsync(string token) => (null, null);

        public async Task AddRefreshTokenAsync(RefreshToken token) { }
    }

    private class FakeUnitOfWork : IUnitOfWork
    {
        public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
            Task.FromResult(1);
    }

    [Fact]
    public async Task UpdateProfile_SetsOnboardingCompleted()
    {
        var userRepo = new FakeUserRepository();
        var uow = new FakeUnitOfWork();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            DisplayName = "Test",
            PasswordHash = "hash",
            CalorieTarget = 2000,
            GoalType = GoalType.Lose,
            CreatedAt = DateTime.UtcNow
        };
        userRepo.Add(user);

        var handler = new UpdateProfileCommandHandler(userRepo, uow);

        // Initially false
        Assert.False(user.OnboardingCompleted);

        var command = new UpdateProfileCommand(user.Id, null, null, null, null, null, true);
        var result = await handler.Handle(command);

        Assert.True(result.OnboardingCompleted);
        Assert.True(user.OnboardingCompleted);
    }

    [Fact]
    public async Task UpdateProfile_DoesNotResetOnboardingCompleted_WhenNotProvided()
    {
        var userRepo = new FakeUserRepository();
        var uow = new FakeUnitOfWork();

        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@test.com",
            DisplayName = "Test",
            PasswordHash = "hash",
            CalorieTarget = 2000,
            GoalType = GoalType.Lose,
            OnboardingCompleted = true,
            CreatedAt = DateTime.UtcNow
        };
        userRepo.Add(user);

        var handler = new UpdateProfileCommandHandler(userRepo, uow);

        // Update display name only — onboarding should remain true
        var command = new UpdateProfileCommand(user.Id, "New Name", null, null, null, null, null);
        var result = await handler.Handle(command);

        Assert.True(result.OnboardingCompleted);
        Assert.Equal("New Name", result.DisplayName);
    }
}
