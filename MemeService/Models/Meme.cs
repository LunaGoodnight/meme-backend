using System.ComponentModel.DataAnnotations;

namespace MemeService.Models;

public class Meme
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Image URL is required")]
    [Url(ErrorMessage = "Invalid URL format")]
    [MaxLength(2000, ErrorMessage = "URL cannot exceed 2000 characters")]
    public required string ImageUrl { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    [MaxLength(10, ErrorMessage = "Maximum 10 keywords allowed")]
    public List<string> Keywords { get; set; } = new List<string>(); 
    
}
