using MemeService.Models;
using Microsoft.EntityFrameworkCore;

namespace MemeService.Data;

public class MemeContext : DbContext
{
    public MemeContext(DbContextOptions<MemeContext> options) : base(options) { }
    public DbSet<Meme> Memes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Meme>()
            .Property(e => e.Keywords)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList());
    }
}