# Meme Backend - Cloud Deployment Checklist

## 🚨 CRITICAL SECURITY FIXES (MUST DO BEFORE DEPLOYMENT)

### 1. Remove Exposed AWS Credentials
- [ ] **URGENT**: Remove hardcoded credentials from `appsettings.json` lines 13-15
- [ ] Remove credentials from Git history: `git filter-branch` or BFG Repo-Cleaner
- [ ] Regenerate DigitalOcean Spaces keys (current ones are compromised)
- [ ] Use environment variables instead:
  ```json
  "AWS": {
    "AccessKey": "${AWS_ACCESS_KEY}",
    "SecretKey": "${AWS_SECRET_KEY}",
    "ServiceURL": "${AWS_SERVICE_URL}"
  }
  ```

### 2. Add CORS Support for Frontend
**Location**: `Program.cs`
```csharp
// Add after line 8
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://your-frontend-domain.com", "http://localhost:3000")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add after line 29, before app.MapControllers()
app.UseCors("AllowFrontend");
```

### 3. Add Authentication/Authorization
- [ ] Consider adding JWT authentication
- [ ] Implement rate limiting
- [ ] Add API key protection for admin operations

## 🔧 ESSENTIAL CLOUD DEPLOYMENT FEATURES

### 4. Environment Configuration
**Create**: `appsettings.Production.json`
```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ConnectionStrings": { 
    "DefaultConnection": "${DATABASE_CONNECTION_STRING}"
  }
}
```

### 5. Health Checks
**Add to** `Program.cs`:
```csharp
// After AddDbContext
builder.Services.AddHealthChecks()
    .AddDbContext<MemeContext>()
    .AddCheck("s3", () => HealthCheckResult.Healthy());

// Before app.Run()
app.MapHealthChecks("/health");
```

### 6. Global Error Handling
**Create**: `Middleware/GlobalExceptionMiddleware.cs`
```csharp
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        context.Response.StatusCode = 500;

        var response = new
        {
            message = "An error occurred while processing your request.",
            statusCode = 500
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
}
```

### 7. Database Migration on Startup
**Add to** `Program.cs`:
```csharp
// Before app.Run()
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<MemeContext>();
    context.Database.Migrate();
}
```

## 📋 PRODUCTION READINESS IMPROVEMENTS

### 8. API Documentation
**Add Swagger**:
```csharp
// In Program.cs
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// In Configure
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
```

### 9. Input Validation Enhancement
**Update** `MemesController.cs`:
```csharp
[HttpPost]
public async Task<ActionResult<Meme>> CreateMeme(Meme meme)
{
    if (!ModelState.IsValid)
        return BadRequest(ModelState);
        
    _context.Memes.Add(meme);
    await _context.SaveChangesAsync();
    return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme);
}
```

### 10. Rate Limiting
**Add package**: `Microsoft.AspNetCore.RateLimiting`
```csharp
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
        RateLimitPartition.GetFixedWindowLimiter("global", _ =>
            new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            }));
});
```

## 🚀 CLOUD PLATFORM SPECIFIC

### For Azure App Service
- [ ] Add `WEBSITES_PORT=80` environment variable
- [ ] Configure Application Insights
- [ ] Set up Azure Key Vault for secrets

### For AWS ECS/Elastic Beanstalk
- [ ] Configure ALB health check path: `/health`
- [ ] Set up AWS Secrets Manager
- [ ] Configure CloudWatch logging

### For Google Cloud Run
- [ ] Set `PORT` environment variable
- [ ] Configure Google Secret Manager
- [ ] Set up Cloud Logging

### For DigitalOcean App Platform
- [ ] Configure health check endpoint
- [ ] Set up environment variables in dashboard
- [ ] Configure managed database connection

## 🔍 TESTING BEFORE DEPLOYMENT

### Local Testing Checklist
- [ ] `dotnet build` succeeds
- [ ] `dotnet run` starts without errors
- [ ] API endpoints respond correctly:
  - `GET /api/memes` - returns 200
  - `POST /api/memes` - creates meme
  - `GET /health` - returns healthy status
- [ ] Database migrations apply successfully
- [ ] CORS works with frontend localhost
- [ ] No credentials in source code

### Docker Testing
- [ ] `docker compose up --build` works
- [ ] API accessible on `http://localhost:5001`
- [ ] Database connection successful
- [ ] Environment variables loaded correctly

## 📚 ENVIRONMENT VARIABLES REFERENCE

```bash
# Database
DATABASE_CONNECTION_STRING=server=your-db-host;database=MemeDB;user=dbuser;password=dbpass

# AWS/DigitalOcean Spaces
AWS_ACCESS_KEY=your-new-access-key
AWS_SECRET_KEY=your-new-secret-key
AWS_SERVICE_URL=https://your-space.region.digitaloceanspaces.com

# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:80
```

## ⚡ QUICK START DEPLOYMENT ORDER

1. **Security First**: Remove credentials, add CORS
2. **Environment Setup**: Configure environment variables
3. **Health & Error**: Add health checks and error handling
4. **Test Locally**: Verify everything works
5. **Deploy**: Push to your cloud platform
6. **Monitor**: Set up logging and monitoring

---

**Status**: 🔴 Not ready for production deployment
**Priority**: Fix security issues first, then add CORS for frontend integration