namespace AnotherUrlShortener.API.Services;

public interface IClickService
{
    Task LogClickAsync(Guid urlId, string? referrer, string? ip);
}