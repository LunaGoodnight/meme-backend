namespace MemeService.Models;

public class MemeUploadRequest
{
    public IFormFile ImageFile { get; set; } = null!;
    public string Keywords { get; set; } = string.Empty;
}