using System.ComponentModel.DataAnnotations;

namespace AnotherUrlShortener.API.Dtos;

public record UrlCreateDto(
    [property: Required, Url] string OriginalUrl,
    DateTime? ExpiresAt = null
);