using System.Text;
using System.Threading.RateLimiting;
using EatCompanion.Api.Middleware;
using EatCompanion.Application.Interfaces;
using EatCompanion.Application.UseCases.Analytics;
using EatCompanion.Application.UseCases.Auth;
using EatCompanion.Application.UseCases.GroceryLists;
using EatCompanion.Application.UseCases.MealLogs;
using EatCompanion.Application.UseCases.MealPlans;
using EatCompanion.Application.UseCases.Profile;
using EatCompanion.Application.UseCases.WeightEntries;
using EatCompanion.Application.Validators;
using EatCompanion.Application.Common;
using EatCompanion.Domain.Interfaces;
using EatCompanion.Infrastructure.Persistence;
using EatCompanion.Infrastructure.Persistence.Repositories;
using EatCompanion.Infrastructure.Services;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

// Serilog
builder.Host.UseSerilog((ctx, lc) => lc.ReadFrom.Configuration(ctx.Configuration));

// DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Repositories
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IMealPlanRepository, MealPlanRepository>();
builder.Services.AddScoped<IGroceryListRepository, GroceryListRepository>();
builder.Services.AddScoped<IMealLogRepository, MealLogRepository>();
builder.Services.AddScoped<IWeightEntryRepository, WeightEntryRepository>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();

// Services
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IPdfParsingService, ClaudePdfParsingService>();
builder.Services.AddScoped<IGroceryListGenerator, GroceryListGenerator>();
builder.Services.AddScoped<INutritionCalculator, NutritionCalculator>();

// Use case handlers
builder.Services.AddScoped<RegisterCommandHandler>();
builder.Services.AddScoped<LoginCommandHandler>();
builder.Services.AddScoped<RefreshTokenCommandHandler>();
builder.Services.AddScoped<GetMealPlansQueryHandler>();
builder.Services.AddScoped<GetMealPlanQueryHandler>();
builder.Services.AddScoped<GetDailySummaryQueryHandler>();
builder.Services.AddScoped<SelectMealOptionCommandHandler>();
builder.Services.AddScoped<ImportMealPlanCommandHandler>();
builder.Services.AddScoped<UpdateMealOptionCommandHandler>();
builder.Services.AddScoped<AddIngredientCommandHandler>();
builder.Services.AddScoped<UpdateIngredientCommandHandler>();
builder.Services.AddScoped<DeleteIngredientCommandHandler>();
builder.Services.AddScoped<LogMealCommandHandler>();
builder.Services.AddScoped<GetMealLogsQueryHandler>();
builder.Services.AddScoped<GetGroceryListQueryHandler>();
builder.Services.AddScoped<GetLatestGroceryListQueryHandler>();
builder.Services.AddScoped<ToggleGroceryItemCommandHandler>();
builder.Services.AddScoped<GenerateGroceryListCommandHandler>();
builder.Services.AddScoped<GetWeeklyAdherenceQueryHandler>();
builder.Services.AddScoped<GetWeightTrendQueryHandler>();
builder.Services.AddScoped<AddWeightEntryCommandHandler>();
builder.Services.AddScoped<UpdateWeightEntryCommandHandler>();
builder.Services.AddScoped<GetWeightEntriesQueryHandler>();
builder.Services.AddScoped<GetProfileQueryHandler>();
builder.Services.AddScoped<UpdateProfileCommandHandler>();

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Secret"]!)),
            ValidateIssuer = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateAudience = true,
            ValidAudience = builder.Configuration["Jwt:Audience"],
            ClockSkew = TimeSpan.Zero
        };
    });

// CORS
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        var origins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>();
        if (origins is not null)
        {
            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        }
    });
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: context.Connection.RemoteIpAddress?.ToString() ?? "anonymous",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterCommandValidator>();

// Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
builder.Services.AddOpenApi();

var app = builder.Build();

// Middleware pipeline
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseRateLimiter();
app.MapControllers();

app.Run();
