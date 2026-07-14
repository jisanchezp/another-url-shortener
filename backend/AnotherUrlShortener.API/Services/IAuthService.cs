using AnotherUrlShortener.API.Dtos;

namespace AnotherUrlShortener.API.Services;

public interface IAuthService
{
    string? GenerateToken(UserDto userDto);
}