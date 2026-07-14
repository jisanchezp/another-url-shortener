using AnotherUrlShortener.API.Data;
using AnotherUrlShortener.API.Dtos;
using AnotherUrlShortener.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AnotherUrlShortenerDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("UrlShortenerDb")
        ?? throw new InvalidOperationException("Connection string is null."))
        .UseSnakeCaseNamingConvention();
});

builder.Services.AddAuthentication(opt =>
{
    opt.DefaultAuthenticateScheme = "JwtBearer";
    opt.DefaultChallengeScheme = "JwtBearer";
}).AddJwtBearer("JwtBearer", opt =>
{
    opt.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] 
            ?? throw new InvalidOperationException("JWT Key is null."))
        )
    };
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.MapGet("/GetHello", [Authorize] () =>
{
    return "Hello, World!";
})
.WithName("GetHello");

app.MapPost("/signup", async (UserSignupDto userSignupDto, IAuthService authService, IUserService userService) =>
{
    var result = await userService.CreateAsync(userSignupDto);

    if (result.IsSuccess == false || result.Value == default)
    {
        return Results.Conflict(new { message = result.Error });
    }
    
    var token = authService.GenerateToken(result.Value); 
    return Results.Ok(new AuthResponseDto(token, result.Value));
});

app.Run();