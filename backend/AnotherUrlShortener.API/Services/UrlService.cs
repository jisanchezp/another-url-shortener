using System.Security.Cryptography;
using AnotherUrlShortener.API.Common;
using AnotherUrlShortener.API.Data;
using AnotherUrlShortener.API.Dtos;
using AnotherUrlShortener.API.Mappers;
using AnotherUrlShortener.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnotherUrlShortener.API.Services;

public class Urlservice : IUrlService
{
    private readonly AnotherUrlShortenerDbContext _dbContext;
    private IConfiguration _configuration;

    public Urlservice(AnotherUrlShortenerDbContext dbContext, IConfiguration configuration)
    {
        _dbContext = dbContext;        
        _configuration = configuration;
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
                return Result<UrlDto>.Success(url.ToUrlDto(_configuration["App:BaseUrl"]
                    ?? throw new InvalidOperationException("Base Url is null.")));
            }
            catch (DbUpdateException ex) when (ex.IsUniqueViolation())
            {
                _dbContext.Entry(url).State = EntityState.Detached;
                continue;
            }
        }
        
        return Result<UrlDto>.Failure("Failed to generate an unique slug.");
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