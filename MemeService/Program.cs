using Amazon.S3;
using MemeService.Data;
using MemeService.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Entity Framework
builder.Services.AddDbContext<MemeContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21)),
    o => o.EnableRetryOnFailure()));

// Configure S3 with settings from environment variables
builder.Services.AddSingleton<IAmazonS3>(s =>
{
    var accessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY") ?? 
                   Environment.GetEnvironmentVariable("AWS__AccessKey") ?? 
                   builder.Configuration["AWS:AccessKey"];
    var secretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY") ?? 
                   Environment.GetEnvironmentVariable("AWS__SecretKey") ?? 
                   builder.Configuration["AWS:SecretKey"];
    var serviceUrl = Environment.GetEnvironmentVariable("AWS_SERVICE_URL") ?? 
                    Environment.GetEnvironmentVariable("AWS__ServiceURL") ?? 
                    builder.Configuration["AWS:ServiceURL"];
    
    return new AmazonS3Client(accessKey, secretKey, new AmazonS3Config
    {
        ServiceURL = serviceUrl,
        ForcePathStyle = true
    });
});

// Register image upload service
builder.Services.AddScoped<IImageUploadService, ImageUploadService>();

var app = builder.Build();

// Apply database migrations on startup
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MemeContext>();
    context.Database.Migrate();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseRouting();

app.MapControllers(); // Enables attribute routing for your API controllers

app.Run();