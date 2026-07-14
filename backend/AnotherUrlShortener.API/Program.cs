using AnotherUrlShortener.API.Data;
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

app.Run();