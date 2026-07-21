using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.RateLimiting;
using AnotherUrlShortener.API.Common;
using AnotherUrlShortener.API.Data;
using AnotherUrlShortener.API.Dtos;
using AnotherUrlShortener.API.Services;
using AnotherUrlShortener.API.Services.Background;
using Microsoft.AspNetCore.RateLimiting;
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

builder.Services.AddRateLimiter(options => 
    options.AddFixedWindowLimiter(policyName: "fixed", limiterOptions =>
        {
            limiterOptions.PermitLimit = 20;
            limiterOptions.Window = TimeSpan.FromMinutes(60);
            limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOptions.QueueLimit = 0;

        })
        .OnRejected = async (context, token) => {
            context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            await context.HttpContext.Response.WriteAsJsonAsync(
                new { message = "Rate limit exceeded. Try again later." }, cancellationToken: token
            );
        });

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IUrlService, UrlService>();
builder.Services.AddSingleton<IClickService, ClickService>();
builder.Services.AddHostedService<ClickWriterService>();

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
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/api/auth/signup", async (UserSignupDto userSignupDto, IAuthService authService, IUserService userService) =>
{
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

app.MapPost("/api/auth/login", async (UserLoginDto userLoginDto, IAuthService authService, IUserService userService) =>
{
    if (!ValidationHelper.TryValidate(userLoginDto, out var errors))
    {
        return Results.BadRequest(errors);
    }

    var result = await userService.LoginAsync(userLoginDto);
    
    if (!result.IsSuccess || result.Value is null)
        return Results.Unauthorized();

    var token = authService.GenerateToken(result.Value);
    return Results.Ok(new AuthResponseDto(token, result.Value));
});

app.MapPost("/api/urls", async (UrlCreateDto urlCreateDto, ClaimsPrincipal user, IUrlService urlService) =>
{
    if (!ValidationHelper.TryValidate(urlCreateDto, out var errors))
    {
        return Results.BadRequest(errors);
    }


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

app.MapGet("/{slug:regex(^[a-zA-Z0-9]{{6}}$)}", async (string slug, 
    IUrlService urlService, IClickService clickService, HttpContext httpCtx) =>
{
    var result = await urlService.GetBySlugAsync(slug);

    if (!result.IsSuccess || result.Value is null)
    {
        return Results.NotFound();
    }

    _ = clickService.LogClickAsync(
        result.Value.Id,
        httpCtx.Request.Headers.Referer.ToString(),
        httpCtx.Connection.RemoteIpAddress?.ToString());

    return Results.Redirect(result.Value.OriginalUrl, permanent: true);
});

app.Run();