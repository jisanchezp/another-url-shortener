namespace AnotherUrlShortener.API.Dtos;

public record UrlStatsDto(List<DailyCountDto> ClicksByDay, List<ReferrerCountDto> TopReferrers, int TotalClicks, int UniqueVisitorsCount);