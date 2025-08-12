using Amazon.S3;
using MemeService.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

// Add Entity Framework
builder.Services.AddDbContext<MemeContext>(options =>
    options.UseMySql(builder.Configuration.GetConnectionString("DefaultConnection"),
    new MySqlServerVersion(new Version(8, 0, 21))));

// Configure S3 with settings from configuration
builder.Services.AddSingleton<IAmazonS3>(s =>
    new AmazonS3Client(
        builder.Configuration["AWS:AccessKey"],
        builder.Configuration["AWS:SecretKey"],
        new AmazonS3Config
        {
            ServiceURL = builder.Configuration["AWS:ServiceURL"],
            ForcePathStyle = true
        }));

var app = builder.Build();

app.UseHttpsRedirection();
app.UseRouting();

app.MapControllers(); // Enables attribute routing for your API controllers

app.Run();