using AnotherUrlShortener.API.Dtos;
using AnotherUrlShortener.API.Models;

namespace AnotherUrlShortener.API.Mappers;

public static class UrlMapper
{
    public static UrlDto ToUrlDto(this Url url, string baseUrl)
    {
        return new UrlDto(url.Slug, url.OriginalUrl,
            $"{baseUrl}/${url.Slug}", url.CreatedAt,
            (DateTime) url.ExpiresAt!);
    }
}