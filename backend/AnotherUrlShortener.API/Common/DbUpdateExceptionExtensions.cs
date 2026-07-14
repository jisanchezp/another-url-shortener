using Microsoft.EntityFrameworkCore;

namespace AnotherUrlShortener.API.Common;

public static class DbUpdateExceptionExtensions
{
    public static bool IsUniqueViolation(this DbUpdateException ex) =>
        ex.InnerException is Npgsql.PostgresException pgEx && pgEx.SqlState == "23505";
}