namespace AnotherUrlShortener.API.Models;

public class Url
{
    public int Id { get; set; }
    public string Slug { get; set; } = null!;
    public string Original { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
}