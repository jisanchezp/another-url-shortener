using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AnotherUrlShortener.API.Dtos;
using AnotherUrlShortener.API.Models;
using Microsoft.IdentityModel.Tokens;

namespace AnotherUrlShortener.API.Services;

public class AuthService : IAuthService
{
    private IConfiguration _configuration;
    public AuthService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public string GenerateToken(UserDto userDto)
    {
        var key = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("JWT Key is null.")));

        var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
          new (JwtRegisteredClaimNames.Sub, userDto.Id.ToString()),
          new (JwtRegisteredClaimNames.Email, userDto.Email),
          new (ClaimTypes.Name, userDto.Username),
          new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())  
        };

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(5),
            signingCredentials: signingCredentials
        );

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

        return tokenString;
    }
}