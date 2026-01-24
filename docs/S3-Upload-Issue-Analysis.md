# S3/DigitalOcean Spaces Upload Issue - Root Cause Analysis

## Problem
Files were being uploaded to a folder named after the bucket (e.g., `/m/` or `/cute33/`) instead of the root directory of the Space.

## Root Cause
Incorrect `ForcePathStyle` configuration in the S3 client setup.

### What Happened
1. **`ForcePathStyle = true`** was set in `Program.cs`
2. This caused the AWS SDK to use **path-style URLs**: `https://sgp1.digitaloceanspaces.com/cute33/filename.jpg`
3. When the bucket name was included in the URL path, DigitalOcean Spaces interpreted it as a folder prefix
4. Result: Files were uploaded to `/cute33/filename.jpg` (a folder) instead of the root

### The Fix
Changed `ForcePathStyle = false` to use **virtual-hosted style URLs**: `https://cute33.sgp1.digitaloceanspaces.com/filename.jpg`

## Key Learnings

### 1. S3 URL Styles
There are two ways to address S3 buckets:

**Path-Style (deprecated):**
```
https://s3.region.amazonaws.com/bucket-name/object-key
https://sgp1.digitaloceanspaces.com/cute33/file.jpg
```

**Virtual-Hosted Style (recommended):**
```
https://bucket-name.s3.region.amazonaws.com/object-key
https://cute33.sgp1.digitaloceanspaces.com/file.jpg
```

### 2. ForcePathStyle Configuration
- **`ForcePathStyle = true`**: Uses path-style URLs (bucket name in URL path)
- **`ForcePathStyle = false`**: Uses virtual-hosted style URLs (bucket name as subdomain)

### 3. Correct DigitalOcean Spaces Configuration

**ServiceURL should be the REGION endpoint, not the bucket endpoint:**
```csharp
// ❌ WRONG
ServiceURL = "https://cute33.sgp1.digitaloceanspaces.com"

// ✅ CORRECT
ServiceURL = "https://sgp1.digitaloceanspaces.com"
```

**Environment variables:**
```bash
AWS__ServiceURL=https://sgp1.digitaloceanspaces.com  # Region endpoint only
AWS__BucketName=cute33                                # Bucket name separately
```

### 4. How S3 Key Works
The `Key` parameter in `PutObjectRequest` is the **object path within the bucket**:

```csharp
// Upload to root of bucket
Key = "filename.jpg"  → /filename.jpg

// Upload to folder
Key = "folder/filename.jpg"  → /folder/filename.jpg
```

**Important:** The bucket name should NEVER be part of the Key when using proper S3 configuration.

### 5. CDN Configuration
When using DigitalOcean CDN:
- **Origin**: Points to your Space (e.g., `cute33.sgp1.digitaloceanspaces.com`)
- **CDN Endpoint**: The CDN URL (e.g., `cute33.sgp1.cdn.digitaloceanspaces.com`)
- **Custom Domain**: Your subdomain (e.g., `i.vividcats.org`)

Files uploaded to the Space root will be accessible at: `https://i.vividcats.org/filename.jpg`

## Configuration Checklist

- [ ] `ServiceURL` = Region endpoint (e.g., `https://sgp1.digitaloceanspaces.com`)
- [ ] `BucketName` = Your Space name (e.g., `cute33`)
- [ ] `ForcePathStyle = false` for DigitalOcean Spaces
- [ ] CDN configured to point to Space origin
- [ ] Return URL uses CDN domain: `https://i.vividcats.org/{key}`

## References
- [AWS S3 Virtual Hosting](https://docs.aws.amazon.com/AmazonS3/latest/userguide/VirtualHosting.html)
- [DigitalOcean Spaces API](https://docs.digitalocean.com/products/spaces/reference/s3-sdk-examples/)
