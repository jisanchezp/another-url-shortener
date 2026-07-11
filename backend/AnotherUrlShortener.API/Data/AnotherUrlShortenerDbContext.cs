
using Microsoft.EntityFrameworkCore;

namespace AnotherUrlShortener.API.Data;

public partial class AnotherUrlShortenerDbContext : DbContext
{
    public AnotherUrlShortenerDbContext(DbContextOptions<AnotherUrlShortenerDbContext> options)
        : base(options)
    {        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
    } 
}