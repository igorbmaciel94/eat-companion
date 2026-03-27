namespace EatCompanion.Application.DTOs;

public record WeightEntryDto(
    Guid Id,
    DateOnly Date,
    decimal WeightKg,
    DateTime CreatedAt
);
