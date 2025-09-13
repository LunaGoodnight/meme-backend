namespace MemeService.Services;

public interface IImageUploadService
{
    Task<string> UploadImageAsync(IFormFile imageFile);
    Task<bool> DeleteImageAsync(string imageUrl);
}