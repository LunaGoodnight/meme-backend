using Amazon.S3;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddSingleton<IAmazonS3>(s =>
    new AmazonS3Client("SPACES_KEY", "SPACES_SECRET", new AmazonS3Config
    {
        ServiceURL = "https://nyc3.digitaloceanspaces.com", // Change region if needed
        ForcePathStyle = true
    }));
var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.MapControllers(); // Enables attribute routing for your API controllers

app.Run();