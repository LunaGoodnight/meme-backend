namespace MemeService.Models;

public class Meme
{
    public int Id { get; set; }
    public required string ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public List<string> Keywords { get; set; } = new List<string>(); 
    
}
