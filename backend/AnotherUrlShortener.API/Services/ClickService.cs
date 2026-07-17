using System.Security.Cryptography;
using System.Text;
using System.Threading.Channels;

namespace AnotherUrlShortener.API.Services;

public class ClickService : IClickService
{
    private readonly Channel<ClickEvent> _channel = Channel.CreateUnbounded<ClickEvent>();

    public ValueTask LogClickAsync(Guid urlId, string? referrer, string? ip)
    {
        return _channel.Writer.WriteAsync(new ClickEvent(urlId, referrer, HashIp(ip), DateTime.UtcNow));
    }

    public ChannelReader<ClickEvent> Reader => _channel.Reader;

    public record ClickEvent(Guid UrlId, string? Referrer, string? IpHash, DateTime OccurredAt);

    private static string? HashIp(string? ip)
    {
        if (ip is null) return null;

        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(ip));
        return Convert.ToHexString(bytes);
    }    
}