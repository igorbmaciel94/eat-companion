using EatCompanion.Application.Interfaces;
using EatCompanion.Application.UseCases.Analytics;
using EatCompanion.Domain.Entities;
using EatCompanion.Domain.Enums;

namespace EatCompanion.Infrastructure.Tests;

public class WeeklyAdherenceTests
{
    private class FakeMealLogRepository : IMealLogRepository
    {
        public List<MealLog> Logs { get; } = new();

        public Task<List<MealLog>> GetByUserAndDateRangeAsync(Guid userId, DateOnly startDate, DateOnly endDate)
        {
            var result = Logs
                .Where(l => l.UserId == userId && l.Date >= startDate && l.Date <= endDate)
                .OrderBy(l => l.Date)
                .ToList();
            return Task.FromResult(result);
        }

        public Task<MealLog?> GetByUserDateAndOptionAsync(Guid userId, DateOnly date, Guid? mealOptionId)
            => Task.FromResult(Logs.FirstOrDefault(l => l.UserId == userId && l.Date == date && l.MealOptionId == mealOptionId));

        public Task AddAsync(MealLog mealLog) { Logs.Add(mealLog); return Task.CompletedTask; }
        public void Update(MealLog mealLog) { }
        public void Delete(MealLog mealLog) { Logs.Remove(mealLog); }
    }

    private class FakeMealPlanRepository : IMealPlanRepository
    {
        public List<MealPlan> Plans { get; } = new();
        private readonly Dictionary<(Guid, int), MealPlanDay> _daysByDow = new();

        public void AddDay(Guid planId, int dayOfWeek, MealPlanDay day)
        {
            _daysByDow[(planId, dayOfWeek)] = day;
        }

        public Task<MealPlan?> GetByIdAsync(Guid id) => Task.FromResult(Plans.FirstOrDefault(p => p.Id == id));
        public Task<MealPlan?> GetByIdWithDetailsAsync(Guid id) => GetByIdAsync(id);
        public Task<List<MealPlan>> GetByUserIdAsync(Guid userId)
            => Task.FromResult(Plans.Where(p => p.UserId == userId && p.IsActive).ToList());
        public Task<List<MealPlan>> GetByUserIdWithDetailsAsync(Guid userId)
            => Task.FromResult(Plans.Where(p => p.UserId == userId && p.IsActive).ToList());
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

    private static MealPlan CreatePlanWithOption(Guid userId, Guid planId, Guid optionId, int calories)
    {
        var option = new MealOption
        {
            Id = optionId,
            MealId = Guid.NewGuid(),
            Name = "Chicken & Rice",
            Calories = calories,
            ProteinGrams = 30,
            IsSelected = true
        };

        var meal = new Meal
        {
            Id = option.MealId,
            MealType = MealType.Lunch,
            Options = new List<MealOption> { option }
        };

        var day = new MealPlanDay
        {
            Id = Guid.NewGuid(),
            MealPlanId = planId,
            DayOfWeek = (int)DayOfWeek.Thursday,
            Date = DateOnly.FromDateTime(DateTime.UtcNow),
            Meals = new List<Meal> { meal }
        };

        return new MealPlan
        {
            Id = planId,
            UserId = userId,
            Name = "Test Plan",
            IsActive = true,
            Days = new List<MealPlanDay> { day }
        };
    }

    [Fact]
    public async Task Adherence_CountsCompletedMeals_InDailySummary()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var optionId = Guid.NewGuid();

        var plan = CreatePlanWithOption(userId, planId, optionId, 500);
        var planRepo = new FakeMealPlanRepository();
        planRepo.Plans.Add(plan);

        var logRepo = new FakeMealLogRepository();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        logRepo.Logs.Add(new MealLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = today,
            MealType = MealType.Lunch,
            Status = MealLogStatus.Completed,
            MealOptionId = optionId
        });

        var handler = new GetWeeklyAdherenceQueryHandler(logRepo, planRepo);
        var result = await handler.Handle(new GetWeeklyAdherenceQuery(userId, 1));

        Assert.Single(result);
        var week = result[0];

        // Find today's summary
        var todaySummary = week.DailySummaries.FirstOrDefault(d => d.Date == today);
        Assert.NotNull(todaySummary);
        Assert.Equal(1, todaySummary.MealsCompleted);
        Assert.Equal(500, todaySummary.CaloriesConsumed);
    }

    [Fact]
    public async Task Adherence_SkippedMeals_DoNotCountAsCompleted()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var optionId = Guid.NewGuid();

        var plan = CreatePlanWithOption(userId, planId, optionId, 500);
        var planRepo = new FakeMealPlanRepository();
        planRepo.Plans.Add(plan);

        var logRepo = new FakeMealLogRepository();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        logRepo.Logs.Add(new MealLog
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Date = today,
            MealType = MealType.Lunch,
            Status = MealLogStatus.Skipped,
            MealOptionId = optionId
        });

        var handler = new GetWeeklyAdherenceQueryHandler(logRepo, planRepo);
        var result = await handler.Handle(new GetWeeklyAdherenceQuery(userId, 1));

        var todaySummary = result[0].DailySummaries.FirstOrDefault(d => d.Date == today);
        Assert.NotNull(todaySummary);
        Assert.Equal(0, todaySummary.MealsCompleted);
        Assert.Equal(0, todaySummary.CaloriesConsumed);
    }

    [Fact]
    public async Task Adherence_NoLogs_Returns7DaysAllZero()
    {
        var userId = Guid.NewGuid();
        var planRepo = new FakeMealPlanRepository();
        planRepo.Plans.Add(new MealPlan
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Name = "Empty Plan",
            IsActive = true,
            Days = new List<MealPlanDay>()
        });

        var logRepo = new FakeMealLogRepository();
        var handler = new GetWeeklyAdherenceQueryHandler(logRepo, planRepo);
        var result = await handler.Handle(new GetWeeklyAdherenceQuery(userId, 1));

        Assert.Single(result);
        Assert.Equal(7, result[0].DailySummaries.Count);
        Assert.All(result[0].DailySummaries, d =>
        {
            Assert.Equal(0, d.MealsCompleted);
            Assert.Equal(0, d.CaloriesConsumed);
        });
    }

    [Fact]
    public async Task Adherence_CaloriesFromPlanOptions_AreCorrectlySummed()
    {
        var userId = Guid.NewGuid();
        var planId = Guid.NewGuid();
        var option1Id = Guid.NewGuid();
        var option2Id = Guid.NewGuid();

        var option1 = new MealOption { Id = option1Id, MealId = Guid.NewGuid(), Name = "Breakfast", Calories = 350, IsSelected = true };
        var option2 = new MealOption { Id = option2Id, MealId = Guid.NewGuid(), Name = "Lunch", Calories = 600, IsSelected = true };

        var meal1 = new Meal { Id = option1.MealId, MealType = MealType.Breakfast, Options = new List<MealOption> { option1 } };
        var meal2 = new Meal { Id = option2.MealId, MealType = MealType.Lunch, Options = new List<MealOption> { option2 } };

        var plan = new MealPlan
        {
            Id = planId,
            UserId = userId,
            Name = "Test Plan",
            IsActive = true,
            Days = new List<MealPlanDay>
            {
                new MealPlanDay
                {
                    Id = Guid.NewGuid(),
                    MealPlanId = planId,
                    Date = DateOnly.FromDateTime(DateTime.UtcNow),
                    DayOfWeek = (int)DateTime.UtcNow.DayOfWeek,
                    Meals = new List<Meal> { meal1, meal2 }
                }
            }
        };

        var planRepo = new FakeMealPlanRepository();
        planRepo.Plans.Add(plan);

        var logRepo = new FakeMealLogRepository();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        logRepo.Logs.Add(new MealLog { Id = Guid.NewGuid(), UserId = userId, Date = today, MealType = MealType.Breakfast, Status = MealLogStatus.Completed, MealOptionId = option1Id });
        logRepo.Logs.Add(new MealLog { Id = Guid.NewGuid(), UserId = userId, Date = today, MealType = MealType.Lunch, Status = MealLogStatus.Completed, MealOptionId = option2Id });

        var handler = new GetWeeklyAdherenceQueryHandler(logRepo, planRepo);
        var result = await handler.Handle(new GetWeeklyAdherenceQuery(userId, 1));

        var todaySummary = result[0].DailySummaries.First(d => d.Date == today);
        Assert.Equal(2, todaySummary.MealsCompleted);
        Assert.Equal(950, todaySummary.CaloriesConsumed);
    }
}
