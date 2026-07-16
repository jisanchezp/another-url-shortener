using System.ComponentModel.DataAnnotations;

namespace AnotherUrlShortener.API.Dtos;

public record UserLoginDto(
    [property: Required] string Username,
    [property: Required] string Password
);