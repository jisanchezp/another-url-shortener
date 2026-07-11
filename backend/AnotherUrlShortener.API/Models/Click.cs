namespace AnotherUrlShortener.API.Models;

public class Click
{
    public int Guid Id { get; set; } = Guid.CreateVersion7();
    public int UrlId { get; set; }
    public string? Referrer { get; set; } = null!;
    public string IpHash { get; set; } = null!;
    public string? Country { get; set; }
    public DateTime ClickedAt { get; set; }

    public Url Url { get; set; } = null!;
}