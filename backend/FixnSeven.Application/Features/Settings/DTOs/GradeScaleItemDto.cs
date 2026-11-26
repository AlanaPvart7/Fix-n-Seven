namespace FixnSeven.Application.Features.Settings.DTOs;

public record GradeScaleItemDto(
    string Grade,        // Excelente, Bueno, Regular o Reprobado
    int MinScore,
    int MaxScore
);
