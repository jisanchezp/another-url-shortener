namespace AnotherUrlShortener.API.Dtos;

public record UrlCreateDto(string OriginalUrl, DateTime? ExpiresAt = null);