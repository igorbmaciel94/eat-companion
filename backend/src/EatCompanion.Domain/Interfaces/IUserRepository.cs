using EatCompanion.Domain.Entities;

namespace EatCompanion.Domain.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task AddAsync(User user);
    void Update(User user);
    Task<bool> ExistsAsync(string email);
}
