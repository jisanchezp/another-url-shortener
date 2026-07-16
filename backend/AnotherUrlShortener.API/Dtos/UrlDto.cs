namespace AnotherUrlShortener.API.Dtos;

public record UrlDto(Guid Id, string Slug, string OriginalUrl, string ShortenedUrl, DateTime CreatedAt, DateTime ExpiresAt);