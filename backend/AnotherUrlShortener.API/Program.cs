using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using AnotherUrlShortener.API.Common;
using AnotherUrlShortener.API.Data;
using AnotherUrlShortener.API.Dtos;
using AnotherUrlShortener.API.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
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
    opt.MapInboundClaims = false;
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

builder.Services.AddAuthorization();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUrlService, UrlService>();

builder.Services.AddOpenApi();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseExceptionHandler(errApp =>
{
    errApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        await context.Response.WriteAsJsonAsync(new { message = "An unexpected error occurred." });
    });
});

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapGet("/api/GetHello", () =>
{
    return "Hello, World!";
})
.WithName("GetHello")
.RequireAuthorization();

app.MapPost("/api/signup", async (UserSignupDto userSignupDto, IAuthService authService, IUserService userService) =>
{
    var isValid = ValidationHelper.TryValidate(userSignupDto, out var errors1);
    Console.WriteLine($"Valid: {isValid}, Errors: {string.Join(",", errors1)}");

    if (!ValidationHelper.TryValidate(userSignupDto, out var errors))
    {
        return Results.BadRequest(errors);
    }

    var result = await userService.CreateAsync(userSignupDto);

    if (result.IsSuccess == false || result.Value == default)
    {
        return Results.Conflict(new { message = result.Error });
    }
    
    var token = authService.GenerateToken(result.Value); 
    return Results.Ok(new AuthResponseDto(token, result.Value));
});

app.MapPost("/api/login", async (UserLoginDto userLoginDto, IAuthService authService, IUserService userService) =>
{
    var result = await userService.LoginAsync(userLoginDto);
    
    if (!result.IsSuccess || result.Value is null)
        return Results.Unauthorized();

    var token = authService.GenerateToken(result.Value);
    return Results.Ok(new AuthResponseDto(token, result.Value));
});

app.MapPost("/api/url", async (UrlCreateDto urlCreateDto, ClaimsPrincipal user, IUrlService urlService) =>
{
    var userId = user.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;

    if (userId is null || !Guid.TryParse(userId, out var parsedUserId))
    {
        return Results.Unauthorized();
    }

    var result = await urlService.CreateAsync(parsedUserId, urlCreateDto);

    if (!result.IsSuccess || result.Value is null)
    {
        return Results.InternalServerError(result.Error);
    }

    return Results.Created($"/api/url/{result.Value.Id}", result.Value);
}).RequireAuthorization();

app.Run();