using AnotherUrlShortener.API.Dtos;

namespace AnotherUrlShortener.API.Services;

public interface IUrlService
{
    Task<Result<UrlDto>> CreateAsync(Guid userId, UrlCreateDto urlCreateDto);
    Task<Result<UrlDto>> GetBySlugAsync(string slug);
    Task<bool> IsOwnedByUserAsync(Guid id, Guid userId);
}