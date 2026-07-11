using AnotherUrlShortener.API.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<AnotherUrlShortenerDbContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("UrlShortenerDb")
        ?? throw new InvalidOperationException("Connection string is null."))
        .UseSnakeCaseNamingConvention();
});

builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapGet("/GetHello", () =>
{
    return "Hello, World!";
})
.WithName("GetHello");

app.Run();