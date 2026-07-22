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
    private readonly AnotherUrlShortenerDbContext _dbContext;
    public ChannelReader<ClickEvent> Reader => _channel.Reader;
    public record ClickEvent(Guid UrlId, string? Referrer, string? IpHash, DateTime OccurredAt);

    public ClickService(AnotherUrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ValueTask LogClickAsync(Guid urlId, string? referrer, string? ip)
    {
        return _channel.Writer.WriteAsync(new ClickEvent(urlId, referrer, HashIp(ip), DateTime.UtcNow));
    }

    public async Task<UrlStatsDto> GetClickStats(Guid urlId)
    {
        var clicksByDay = await GetClicksByDayAsync(urlId);
        var topReferrers = await GetTopReferrersAsync(urlId);
        var totalClicks = await GetTotalClicksAsync(urlId);
        var uniqueVisitorsCount = await GetUniqueVisitorsCountAsync(urlId);

        return new UrlStatsDto(clicksByDay, topReferrers, totalClicks, uniqueVisitorsCount);
    }

    private async Task<List<DailyCountDto>> GetClicksByDayAsync(Guid urlId)
    {
        return await _dbContext.Clicks
            .Where(c => c.UrlId == urlId)
            .GroupBy(c => c.ClickedAt.Date)
            .Select(g => new DailyCountDto(DateOnly.FromDateTime(g.Key), g.Count()))
            .OrderBy(g => g.Date)
            .ToListAsync();            
    }

    private async Task<List<ReferrerCountDto>> GetTopReferrersAsync(Guid urlId)
    {
        var referrerClicks = await _dbContext.Clicks
            .Where(c => c.UrlId == urlId && c.Referrer != null)
            .ToListAsync();

        var topReferrers = referrerClicks.GroupBy(c => NormalizeReferrer(c.Referrer!))
            .Select(g => new ReferrerCountDto(g.Key, g.Count()))
            .Take(10)
            .OrderByDescending(g => g.Count)            
            .ToList();

        return topReferrers;            
    }

    private async Task<int> GetTotalClicksAsync(Guid urlId)
    {
        return await _dbContext.Clicks
            .Where(c => c.UrlId == urlId)
            .CountAsync();
    }

    private async Task<int> GetUniqueVisitorsCountAsync(Guid urlId)
    {
        return await _dbContext.Clicks
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