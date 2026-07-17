using AnotherUrlShortener.API.Data;
using AnotherUrlShortener.API.Models;

namespace AnotherUrlShortener.API.Services.Background;

public class ClickWriterService : BackgroundService
{
    private readonly IClickService _clickService;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    

    public ClickWriterService(IClickService clickService, IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
        _clickService = clickService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var service = (ClickService)_clickService;
        await foreach (var evt in service.Reader.ReadAllAsync(stoppingToken))
        {
            using var serviceScope = _serviceScopeFactory.CreateScope();
            var _dbContext = serviceScope.ServiceProvider.GetRequiredService<AnotherUrlShortenerDbContext>();

            _dbContext.Clicks.Add(new Click
                {
                    UrlId = evt.UrlId,
                    Referrer = evt.Referrer,
                    IpHash = evt.IpHash,
                    Country = null,
                    ClickedAt = evt.OccurredAt
                });
            await _dbContext.SaveChangesAsync();
        }
    }
}