namespace AnotherUrlShortener.API.Dtos;

public record UrlDto(string Slug, string OriginalUrl, string ShortenedUrl, DateTime CreatedAt, DateTime ExpiresAt);