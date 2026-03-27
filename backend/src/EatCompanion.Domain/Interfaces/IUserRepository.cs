using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<(User? User, RefreshToken? Token)> GetByRefreshTokenAsync(string token);
    Task AddAsync(User user);
    void Update(User user);
}
