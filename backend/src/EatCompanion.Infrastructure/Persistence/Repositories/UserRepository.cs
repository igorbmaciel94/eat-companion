using EatCompanion.Application.Interfaces;
using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatCompanion.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users.FindAsync(id);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<(User? User, RefreshToken? Token)> GetByRefreshTokenAsync(string token)
    {
        var refreshToken = await _context.Set<RefreshToken>()
            .Include(rt => rt.User)
            .FirstOrDefaultAsync(rt => rt.Token == token);

        if (refreshToken is null)
            return (null, null);

        return (refreshToken.User, refreshToken);
    }

    public async Task AddAsync(User user)
    {
        await _context.Users.AddAsync(user);
    }

    public async Task AddRefreshTokenAsync(RefreshToken token)
    {
        await _context.Set<RefreshToken>().AddAsync(token);
    }

    public void Update(User user)
    {
        _context.Users.Update(user);
    }

    public async Task<bool> ExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(u => u.Email == email);
    }
}
