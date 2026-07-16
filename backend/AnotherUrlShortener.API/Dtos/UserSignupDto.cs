using System.ComponentModel.DataAnnotations;

namespace AnotherUrlShortener.API.Dtos;

public record UserSignupDto(
    [property: Required, MinLength(3), MaxLength(50)] string Username,
    [property: Required, 
        RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]{2,}$",
        ErrorMessage = "The Email field is not a valid e-mail address.")] string Email,
    [property: Required, MinLength(8)] string Password);