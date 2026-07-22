using System.Security.Cryptography;
using AnotherUrlShortener.API.Common;
using AnotherUrlShortener.API.Data;
using AnotherUrlShortener.API.Dtos;
using AnotherUrlShortener.API.Mappers;
using AnotherUrlShortener.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnotherUrlShortener.API.Services;

public class UrlService : IUrlService
{
    private readonly AnotherUrlShortenerDbContext _dbContext;
    private IConfiguration _configuration;
    private readonly string _baseUrl;

    public UrlService(AnotherUrlShortenerDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;        
        _configuration = configuration;
        _baseUrl = _configuration["App:BaseUrl"]
            ?? throw new InvalidOperationException("Base Url is null.");
    }

    public async Task<Result<UrlDto>> CreateAsync(Guid userId, UrlCreateDto urlCreateDto)
    {
        const int SLUG_LENGTH = 6;
        const int MAX_ATTEMPS = 5;
        for (int i = 0; i < MAX_ATTEMPS; i++)
        {
            var slug = GenerateSlug(SLUG_LENGTH);
            var url = new Url
            {
                Slug = slug,
                OriginalUrl = urlCreateDto.OriginalUrl,
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = urlCreateDto.ExpiresAt ?? DateTime.UtcNow.AddDays(90)
            };

            _dbContext.Urls.Add(url);

            try
            {
                await _dbContext.SaveChangesAsync();
                return Result<UrlDto>.Success(url.ToUrlDto(_baseUrl));
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                _dbContext.Entry(url).State = EntityState.Detached;
                continue;
            }
        }
        
        return Result<UrlDto>.Failure("Failed to generate an unique slug.");
    }

    public async Task<Result<UrlDto>> GetBySlugAsync(string slug)
    {
        var url = await _dbContext.Urls.FirstOrDefaultAsync(u => u.Slug == slug);

        if (url is null || (url.ExpiresAt.HasValue && url.ExpiresAt < DateTime.UtcNow))
        {
            return Result<UrlDto>.Failure("Url not found.");
        }

        return Result<UrlDto>.Success(url.ToUrlDto(_baseUrl));
    }
    
    public async Task<bool> IsOwnedByUserAsync(Guid urlId, Guid userId)
    {
        return await _dbContext.Urls.AnyAsync(u => u.Id == urlId && u.UserId == userId);
    }

    private static string GenerateSlug(int length)
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        return new string(
            [..
                Enumerable.Range(0, length)
                .Select(_ => chars[RandomNumberGenerator.GetInt32(chars.Length)])
            ]
        );
    }
}