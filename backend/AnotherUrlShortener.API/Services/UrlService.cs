using AnotherUrlShortener.API.Data;
using AnotherUrlShortener.API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace AnotherUrlShortener.API.Services;

public class Urlservice : IUrlService
{
    private readonly AnotherUrlShortenerDbContext _dbContext;
    
    public UrlService(AnotherUrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;        
    }


    public Task<Result<UrlDto>> Create(Guid userId, UrlCreateDto urlCreateDto)
    {
        throw new NotImplementedException();
    }
}