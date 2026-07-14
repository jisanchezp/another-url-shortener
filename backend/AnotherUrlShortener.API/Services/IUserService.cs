using AnotherUrlShortener.API.Dtos;

namespace AnotherUrlShortener.API.Services;

public interface IUserService
{
    Task<Result<UserDto>> CreateAsync(UserSignupDto userSignupDto);
}