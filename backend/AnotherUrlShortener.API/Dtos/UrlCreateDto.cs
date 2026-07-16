using System.ComponentModel.DataAnnotations;

namespace AnotherUrlShortener.API.Dtos;

public record UrlCreateDto(
    [Required, Url] string OriginalUrl,
    DateTime? ExpiresAt = null
);