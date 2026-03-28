using EatCompanion.Application.Common;
using EatCompanion.Application.Interfaces;
using EatCompanion.Application.UseCases.MealPlans;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;

namespace EatCompanion.Infrastructure.Tests;

public class WeeklySummaryTests
{
    private class FakeMealPlanRepository : IMealPlanRepository
    {
        public List<MealPlan> Plans { get; } = new();
        private readonly Dictionary<(Guid, int), MealPlanDay> _daysByDow = new();

        public void SetDay(Guid planId, int dayOfWeek, MealPlanDay day)
        {
            _daysByDow[(planId, dayOfWeek)] = day;
        }

        public Task<MealPlan?> GetByIdAsync(Guid id) => Task.FromResult(Plans.FirstOrDefault(p => p.Id == id));
        public Task<MealPlan?> GetByIdWithDetailsAsync(Guid id) => GetByIdAsync(id);
        public Task<List<MealPlan>> GetByUserIdAsync(Guid userId)
            => Task.FromResult(Plans.Where(p => p.UserId == userId && p.IsActive).ToList());
        public Task<List<MealPlan>> GetByUserIdWithDetailsAsync(Guid userId) => GetByUserIdAsync(userId);
        public Task<MealPlanDay?> GetDayAsync(Guid mealPlanId, DateOnly date)
        {
            var dow = (int)date.DayOfWeek;
            _daysByDow.TryGetValue((mealPlanId, dow), out var day);
            return Task.FromResult(day);
        }
        public Task<MealOption?> GetMealOptionAsync(Guid optionId) => Task.FromResult<MealOption?>(null);
        public Task<List<MealOption>> GetSiblingOptionsAsync(Guid mealId) => Task.FromResult(new List<MealOption>());
        public Task<Ingredient?> GetIngredientAsync(Guid ingredientId) => Task.FromResult<Ingredient?>(null);
        public Task DeactivateAllForUserAsync(Guid userId) => Task.CompletedTask;
        public Task AddAsync(MealPlan mealPlan) { Plans.Add(mealPlan); return Task.CompletedTask; }
        public void Update(MealPlan mealPlan) { }
        public void UpdateOption(MealOption option) { }
        public void DeleteIngredient(Ingredient ingredient) { }
    }

    [Fact]
    public async Task GetWeek_Returns7Days()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var repo = new FakeMealPlanRepository();
        repo.Plans.Add(new MealPlan { Id = planId, UserId = userId, Name = "Plan", IsActive = true });

        var handler = new GetWeeklySummaryQueryHandler(repo);
        var result = await handler.Handle(new GetWeeklySummaryQuery(planId, userId));

        Assert.Equal(7, result.Count);
    }

    [Fact]
    public async Task GetWeek_StartsOnMonday()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var repo = new FakeMealPlanRepository();
        repo.Plans.Add(new MealPlan { Id = planId, UserId = userId, Name = "Plan", IsActive = true });

        var handler = new GetWeeklySummaryQueryHandler(repo);
        var result = await handler.Handle(new GetWeeklySummaryQuery(planId, userId));

        // First day should be Monday
        Assert.Equal(DayOfWeek.Monday, result[0].Date.DayOfWeek);
        // Last day should be Sunday
        Assert.Equal(DayOfWeek.Sunday, result[6].Date.DayOfWeek);
    }

    [Fact]
    public async Task GetWeek_ReturnsMeals_WhenDayHasData()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var option = new MealOption
        {
            Id = Guid.NewGuid(),
            MealId = Guid.NewGuid(),
            Name = "Oatmeal",
            Calories = 350,
            ProteinGrams = 12,
            IsSelected = true
        };
        var meal = new Meal
        {
            Id = option.MealId,
            MealType = MealType.Breakfast,
            SortOrder = 0,
            Options = new List<MealOption> { option }
        };
        var day = new MealPlanDay
        {
            Id = Guid.NewGuid(),
            MealPlanId = planId,
            DayOfWeek = (int)DayOfWeek.Monday,
            Meals = new List<Meal> { meal }
        };

        var repo = new FakeMealPlanRepository();
        repo.Plans.Add(new MealPlan { Id = planId, UserId = userId, Name = "Plan", IsActive = true });
        repo.SetDay(planId, (int)DayOfWeek.Monday, day);

        var handler = new GetWeeklySummaryQueryHandler(repo);
        var result = await handler.Handle(new GetWeeklySummaryQuery(planId, userId));

        var monday = result.First(d => d.Date.DayOfWeek == DayOfWeek.Monday);
        Assert.Single(monday.Meals);
        Assert.Equal(350, monday.CaloriesConsumed);
    }

    [Fact]
    public async Task GetWeek_EmptyMeals_WhenDayHasNoData()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();

        var repo = new FakeMealPlanRepository();
        repo.Plans.Add(new MealPlan { Id = planId, UserId = userId, Name = "Plan", IsActive = true });

        var handler = new GetWeeklySummaryQueryHandler(repo);
        var result = await handler.Handle(new GetWeeklySummaryQuery(planId, userId));

        Assert.All(result, day =>
        {
            Assert.Empty(day.Meals);
            Assert.Equal(0, day.CaloriesConsumed);
        });
    }

    [Fact]
    public async Task GetWeek_ThrowsNotFound_WhenPlanDoesNotExist()
    {
        var repo = new FakeMealPlanRepository();
        var handler = new GetWeeklySummaryQueryHandler(repo);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetWeeklySummaryQuery(Guid.NewGuid(), Guid.NewGuid())));
    }

    [Fact]
    public async Task GetWeek_ThrowsNotFound_WhenPlanBelongsToDifferentUser()
    {
        var planId = Guid.NewGuid();
        var repo = new FakeMealPlanRepository();
        repo.Plans.Add(new MealPlan { Id = planId, UserId = Guid.NewGuid(), Name = "Plan", IsActive = true });

        var handler = new GetWeeklySummaryQueryHandler(repo);

        await Assert.ThrowsAsync<NotFoundException>(() =>
            handler.Handle(new GetWeeklySummaryQuery(planId, Guid.NewGuid())));
    }
}
