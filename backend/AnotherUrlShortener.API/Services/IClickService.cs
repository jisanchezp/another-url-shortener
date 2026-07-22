using AnotherUrlShortener.API.Dtos;

namespace AnotherUrlShortener.API.Services;

public interface IClickService
{
    ValueTask LogClickAsync(Guid urlId, string? referrer, string? ip);
    Task<UrlStatsDto> GetClickStats(Guid urlId);
}