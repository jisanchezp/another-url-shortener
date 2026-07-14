using AnotherUrlShortener.API.Dtos;
using AnotherUrlShortener.API.Models;

namespace AnotherUrlShortener.API.Mappers;

public static class UserMapper
{
    public static User UserSignupDtoToUser(this UserSignupDto userDto)
    {
        return new User
        {
            Username = userDto.Username,
            Email = userDto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(userDto.Password)
        };
    }
}