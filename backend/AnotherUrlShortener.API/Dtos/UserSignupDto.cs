using System.ComponentModel.DataAnnotations;

namespace AnotherUrlShortener.API.Dtos;

public record UserSignupDto(
    [property: Required, MinLength(3), MaxLength(50)] string Username,
    [property: Required, EmailAddress] string Email,
    [property: Required, MinLength(8)] string Password);