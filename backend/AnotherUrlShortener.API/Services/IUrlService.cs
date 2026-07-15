using AnotherUrlShortener.API.Dtos;

namespace AnotherUrlShortener.API.Services;

public interface IUrlService
{
    Task<Result<UrlDto>> Create(Guid userId, UrlCreateDto urlCreateDto);
}