using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;
using AnotherUrlShortener.API.Data;
using AnotherUrlShortener.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AnotherUrlShortener.API.Services;

public class ClickService : IClickService
{
    private readonly Channel<ClickEvent> _channel = Channel.CreateUnbounded<ClickEvent>();    
    private readonly IServiceScopeFactory _serviceScopeFactory;
    public ChannelReader<ClickEvent> Reader => _channel.Reader;
    public record ClickEvent(Guid UrlId, string? Referrer, string? IpHash, DateTime OccurredAt);

    public ClickService(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }

    public ValueTask LogClickAsync(Guid urlId, string? referrer, string? ip)
    {
        return _channel.Writer.WriteAsync(new ClickEvent(urlId, referrer, HashIp(ip), DateTime.UtcNow));
    }

    public async Task<UrlStatsDto> GetClickStats(Guid urlId)
    {
        using var serviceScope = _serviceScopeFactory.CreateScope();
        var dbContext = serviceScope.ServiceProvider.GetRequiredService<AnotherUrlShortenerDbContext>();

        var clicksByDay = await GetClicksByDayAsync(dbContext, urlId);
        var topReferrers = await GetTopReferrersAsync(dbContext, urlId);
        var totalClicks = await GetTotalClicksAsync(dbContext, urlId);
        var uniqueVisitorsCount = await GetUniqueVisitorsCountAsync(dbContext, urlId);

        return new UrlStatsDto(clicksByDay, topReferrers, totalClicks, uniqueVisitorsCount);
    }

    private async Task<List<DailyCountDto>> GetClicksByDayAsync(AnotherUrlShortenerDbContext dbContext, Guid urlId)
    {
        var clickGroups = await dbContext.Clicks
            .Where(c => c.UrlId == urlId)
            .GroupBy(c => c.ClickedAt).ToListAsync();
            
        return clickGroups
            .Select(g => new DailyCountDto(DateOnly.FromDateTime(g.Key), g.Count()))
            .OrderBy(g => g.Date).ToList(); 
    }

    private async Task<List<ReferrerCountDto>> GetTopReferrersAsync(AnotherUrlShortenerDbContext dbContext, Guid urlId)
    {
        var referrerClicks = await dbContext.Clicks
            .Where(c => c.UrlId == urlId && c.Referrer != null)
            .ToListAsync();

        var topReferrers = referrerClicks.GroupBy(c => NormalizeReferrer(c.Referrer!))
            .Select(g => new ReferrerCountDto(g.Key, g.Count()))
            .Take(10)
            .OrderByDescending(g => g.Count)            
            .ToList();

        return topReferrers;            
    }

    private async Task<int> GetTotalClicksAsync(AnotherUrlShortenerDbContext dbContext, Guid urlId)
    {
        return await dbContext.Clicks
            .Where(c => c.UrlId == urlId)
            .CountAsync();
    }

    private async Task<int> GetUniqueVisitorsCountAsync(AnotherUrlShortenerDbContext dbContext, Guid urlId)
    {
        return await dbContext.Clicks
            .Where(c => c.UrlId == urlId && c.IpHash != null)
            .Select(c => c.IpHash)
            .Distinct()
            .CountAsync();
    }

    private static string? HashIp(string? ip)
    {
        if (ip is null) return null;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes);
    }

    private static string NormalizeReferrer(string referrer)
    {
        return Uri.TryCreate(referrer, UriKind.Absolute, out var uri) ?
            uri.Host : referrer;
    }
}