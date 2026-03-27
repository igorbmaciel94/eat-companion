using EatCompanion.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EatCompanion.Infrastructure.Persistence;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<MealPlan> MealPlans => Set<MealPlan>();
    public DbSet<MealPlanDay> MealPlanDays => Set<MealPlanDay>();
    public DbSet<Meal> Meals => Set<Meal>();
    public DbSet<MealOption> MealOptions => Set<MealOption>();
    public DbSet<Ingredient> Ingredients => Set<Ingredient>();
    public DbSet<GroceryList> GroceryLists => Set<GroceryList>();
    public DbSet<GroceryItem> GroceryItems => Set<GroceryItem>();
    public DbSet<MealLog> MealLogs => Set<MealLog>();
    public DbSet<WeightEntry> WeightEntries => Set<WeightEntry>();
    public DbSet<ImportJob> ImportJobs => Set<ImportJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
