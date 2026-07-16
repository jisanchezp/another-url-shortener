using System.ComponentModel.DataAnnotations;

namespace AnotherUrlShortener.API.Dtos;

public record UserLoginDto(
    [Required, MinLength(3), MaxLength(50)] string Username,
    [Required, MinLength(8)] string Password
);