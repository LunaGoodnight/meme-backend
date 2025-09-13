using Amazon.S3;
using Amazon.S3.Model;

namespace MemeService.Services;

public class ImageUploadService : IImageUploadService
{
    private readonly IAmazonS3 _s3Client;
    private readonly IConfiguration _configuration;
    private readonly string _bucketName;

    public ImageUploadService(IAmazonS3 s3Client, IConfiguration configuration)
    {
        _s3Client = s3Client;
        _configuration = configuration;
        _bucketName = Environment.GetEnvironmentVariable("AWS_BUCKET_NAME") ?? 
                     Environment.GetEnvironmentVariable("AWS__BucketName") ?? 
                     configuration["AWS:BucketName"] ?? 
                     "cute33"; // Default based on your current setup
    }

    public async Task<string> UploadImageAsync(IFormFile imageFile)
    {
        if (imageFile == null || imageFile.Length == 0)
            throw new ArgumentException("Image file is required");

        ValidateImageFile(imageFile);

        var key = GenerateUniqueFileName(imageFile.FileName);
        
        using var stream = imageFile.OpenReadStream();
        
        var request = new PutObjectRequest
        {
            BucketName = _bucketName,
            Key = key,
            InputStream = stream,
            ContentType = imageFile.ContentType,
            CannedACL = S3CannedACL.PublicRead
        };

        await _s3Client.PutObjectAsync(request);
        
        return $"{_configuration["AWS:ServiceURL"]}/{_bucketName}/{key}";
    }

    public async Task<bool> DeleteImageAsync(string imageUrl)
    {
        try
        {
            var key = ExtractKeyFromUrl(imageUrl);
            if (string.IsNullOrEmpty(key)) return false;

            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private void ValidateImageFile(IFormFile imageFile)
    {
        var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif", "image/webp" };
        if (!allowedTypes.Contains(imageFile.ContentType.ToLower()))
            throw new ArgumentException("Only JPEG, PNG, GIF, and WebP images are allowed");

        const int maxSizeInBytes = 10 * 1024 * 1024; // 10MB
        if (imageFile.Length > maxSizeInBytes)
            throw new ArgumentException("Image size cannot exceed 10MB");
    }

    private string GenerateUniqueFileName(string originalFileName)
    {
        var extension = Path.GetExtension(originalFileName);
        var fileName = $"{Guid.NewGuid()}{extension}";
        return fileName;
    }

    private string ExtractKeyFromUrl(string imageUrl)
    {
        try
        {
            var uri = new Uri(imageUrl);
            var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
            return segments.Length > 1 ? segments[^1] : string.Empty;
        }
        catch
        {
            return string.Empty;
        }
    }
}