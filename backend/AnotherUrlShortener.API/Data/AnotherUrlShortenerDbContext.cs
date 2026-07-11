
using AnotherUrlShortener.API.Models;
using Microsoft.EntityFrameworkCore;

namespace AnotherUrlShortener.API.Data;

public partial class AnotherUrlShortenerDbContext : DbContext
{
    public DbSet<User> Users => Set<User>();
    public DbSet<Url> Urls => Set<Url>();
    public DbSet<Click> Clicks => Set<Click>();
    
    public AnotherUrlShortenerDbContext(DbContextOptions<AnotherUrlShortenerDbContext> options)
        : base(options)
    {        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Url>()
            .HasIndex(u => u.Slug)
            .IsUnique();

        modelBuilder.Entity<Click>()
            .HasIndex(c => c.UrlId);
    } 
}