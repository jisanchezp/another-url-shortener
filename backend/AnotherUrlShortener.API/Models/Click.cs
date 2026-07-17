namespace AnotherUrlShortener.API.Models;

public class Click
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public Guid UrlId { get; set; }
    public string? Referrer { get; set; } = null!;
    public string? IpHash { get; set; } = null!;
    public string? Country { get; set; } = null!;
    public DateTime ClickedAt { get; set; }

    public Url Url { get; set; } = null!;
}