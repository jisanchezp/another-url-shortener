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

    public async Task<List<DailyCountDto>> GetClicksByDayAsync(Guid urlId)
    {
        return await _dbContext.Clicks
            .Where(c => c.UrlId == urlId)
            .GroupBy(c => c.ClickedAt.Date)
            .Select(g => new DailyCountDto(DateOnly.FromDateTime(g.Key), g.Count()))
            .OrderBy(g => g.Date)
            .ToListAsync();            
    }

    private static string? HashIp(string? ip)
    {
        if (ip is null) return null;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes);
    }
}