namespace FixnSeven.Application.Common.DTOs;

public record PagedResponseDto<T>(List<T> Items, int Total, int Page, int PageSize);