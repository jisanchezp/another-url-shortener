namespace AnotherUrlShortener.API.Models;

public class Url
{
    public Guid Id { get; set; } = Guid.CreateVersion7();
    public string Slug { get; set; } = null!;
    public string OriginalUrl { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}