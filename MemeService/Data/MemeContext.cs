using MemeService.Models;
using Microsoft.EntityFrameworkCore;

namespace MemeService.Data;

public class MemeContext : DbContext
{
    public MemeContext(DbContextOptions<MemeContext> options) : base(options) { }
    public DbSet<Meme> Memes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var listComparer = new Microsoft.EntityFrameworkCore.ChangeTracking.ValueComparer<List<string>>(
            (c1, c2) => c1.SequenceEqual(c2),
            c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
            c => c.ToList());

        modelBuilder.Entity<Meme>()
            .Property(e => e.Keywords)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList())
            .Metadata.SetValueComparer(listComparer);
    }
}