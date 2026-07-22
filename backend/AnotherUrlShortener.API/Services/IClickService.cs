using AnotherUrlShortener.API.Dtos;

namespace AnotherUrlShortener.API.Services;

public interface IClickService
{
    ValueTask LogClickAsync(Guid urlId, string? referrer, string? ip);
    Task<List<DailyCountDto>> GetClicksByDayAsync(Guid urlId);
    Task<List<ReferrerCountDto>> GetTopReferrersAsync(Guid urlId);
    Task<int> GetTotalClicksAsync(Guid urlId);
    Task<int> GetUniqueVisitorsCountAsync(Guid urlId);
}