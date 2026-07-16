using System.ComponentModel.DataAnnotations;

namespace AnotherUrlShortener.API.Dtos;

public record UserLoginDto(
    [property: Required, MinLength(3), MaxLength(50)] string Username,
    [property: Required, MinLength(8)] string Password
);