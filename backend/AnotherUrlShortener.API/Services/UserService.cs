using AnotherUrlShortener.API.Common;
using AnotherUrlShortener.API.Data;
using AnotherUrlShortener.API.Dtos;
using AnotherUrlShortener.API.Mappers;
using Microsoft.EntityFrameworkCore;

namespace AnotherUrlShortener.API.Services;

public class UserService : IUserService
{
    private readonly AnotherUrlShortenerDbContext _dbContext;

    public UserService(AnotherUrlShortenerDbContext dbContext)
    {
        _dbContext = dbContext;
    }    

    public async Task<Result<UserDto>> CreateAsync(UserSignupDto userSignupDto)
    {
        var user = userSignupDto.ToUser();
        _dbContext.Users.Add(user);

        try
        {
            await _dbContext.SaveChangesAsync();
            return Result<UserDto>.Success(user.ToUserDto());
        }
        catch (DbUpdateException ex) when (ex.IsUniqueViolation())
        {
            return Result<UserDto>.Failure("Username or email already in use.");
        }
    }
}