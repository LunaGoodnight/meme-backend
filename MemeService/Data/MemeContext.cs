using MemeService.Models;
using Microsoft.EntityFrameworkCore;

namespace MemeService.Data;

public class MemeContext : DbContext
{
    public MemeContext(DbContextOptions<MemeContext> options) : base(options) { }
    public DbSet<Meme> Memes { get; set; }
}