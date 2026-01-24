# Image Upload API Documentation

This document describes the image upload functionality for the Meme Service API, which integrates with DigitalOcean Spaces for cloud storage.

## Overview

The Meme Service now supports direct image upload functionality, allowing users to upload images that are automatically stored in DigitalOcean Spaces and associated with meme records in the database.

## Features

- **Multi-format Support**: JPEG, PNG, GIF, and WebP image formats
- **File Size Validation**: Maximum 10MB per image
- **Automatic File Management**: Unique filename generation and storage
- **Cloud Storage Integration**: Direct upload to DigitalOcean Spaces
- **Public Access**: Uploaded images are publicly accessible via URL
- **Cleanup on Delete**: Images are automatically removed from storage when memes are deleted

## Configuration

### Environment Variables

The API supports multiple environment variable formats for DigitalOcean Spaces integration:

**Standard Format:**
```bash
AWS_ACCESS_KEY=your_spaces_access_key
AWS_SECRET_KEY=your_spaces_secret_key
AWS_SERVICE_URL=https://your-region.digitaloceanspaces.com
AWS_BUCKET_NAME=your-bucket-name  # Optional, auto-detected from URL
```

**Double Underscore Format (.env files):**
```bash
AWS__AccessKey=your_spaces_access_key
AWS__SecretKey=your_spaces_secret_key
AWS__ServiceURL=https://your-region.digitaloceanspaces.com
AWS__BucketName=your-bucket-name  # Optional, auto-detected from URL
```

**Example .env file:**
```bash
# DigitalOcean Spaces Configuration
AWS__AccessKey=DO801UUEJWFZYP4GX892
AWS__SecretKey=L+/H3wT2xbES+fRVmfsAxHf6C9NqSKy3paHLvfBjHdw
AWS__ServiceURL=https://cute33.sgp1.digitaloceanspaces.com
```

### Configuration File (appsettings.json)

Alternatively, configure in your `appsettings.json`:

```json
{
  "AWS": {
    "AccessKey": "your_spaces_access_key",
    "SecretKey": "your_spaces_secret_key",
    "ServiceURL": "https://your-region.digitaloceanspaces.com",
    "BucketName": "your-bucket-name"
  }
}
```

### Configuration Priority

The system checks configuration sources in this order:
1. Standard environment variables (`AWS_ACCESS_KEY`)
2. Double underscore environment variables (`AWS__AccessKey`) 
3. Configuration file (`appsettings.json`)
4. Default values (bucket name auto-detected from ServiceURL)

### DigitalOcean Spaces Setup

1. Create a DigitalOcean Spaces bucket (any name - will be auto-detected from ServiceURL)
2. Generate API keys with read/write permissions
3. Configure CORS settings if needed for web applications
4. Note: Bucket name is automatically extracted from the ServiceURL or can be explicitly set via `AWS__BucketName`

## API Endpoints

### Upload Image and Create Meme

Upload an image file and create a new meme record.

**Endpoint:** `POST /api/memes/upload`

**Content-Type:** `multipart/form-data`

**Parameters:**
- `imageFile` (required): The image file to upload
- `keywords` (optional): Comma-separated list of keywords (max 10)

**Example Request (cURL):**

```bash
curl -X POST "http://localhost:5001/api/memes/upload" \
  -H "Content-Type: multipart/form-data" \
  -F "imageFile=@/path/to/your/image.jpg" \
  -F "keywords=funny,cat,meme,internet"
```

**Example Request (JavaScript):**

```javascript
const formData = new FormData();
formData.append('imageFile', imageFile);
formData.append('keywords', 'funny,cat,meme');

const response = await fetch('/api/memes/upload', {
  method: 'POST',
  body: formData
});

const meme = await response.json();
```

**Response (201 Created):**

```json
{
  "id": 1,
  "imageUrl": "https://cute33.sgp1.digitaloceanspaces.com/cute33/550e8400-e29b-41d4-a716-446655440000.jpg",
  "createdAt": "2024-01-15T10:30:00Z",
  "keywords": ["funny", "cat", "meme", "internet"]
}
```

**Error Responses:**

```json
// 400 Bad Request - Invalid file format
{
  "error": "Only JPEG, PNG, GIF, and WebP images are allowed"
}

// 400 Bad Request - File too large
{
  "error": "Image size cannot exceed 10MB"
}

// 400 Bad Request - No file provided
{
  "error": "Image file is required"
}

// 500 Internal Server Error - Upload failed
{
  "error": "An error occurred while uploading the image"
}
```

### Standard CRUD Operations

The existing CRUD endpoints remain unchanged:

- `GET /api/memes` - Get all memes
- `GET /api/memes/{id}` - Get specific meme
- `POST /api/memes` - Create meme with URL (no file upload)
- `PUT /api/memes/{id}` - Update meme
- `DELETE /api/memes/{id}` - Delete meme (also removes image from storage)

## File Validation Rules

### Supported Formats
- JPEG (.jpg, .jpeg)
- PNG (.png)
- GIF (.gif)
- WebP (.webp)

### Size Limits
- Maximum file size: 10MB (10,485,760 bytes)
- No minimum size restriction

### File Naming
- Original filenames are replaced with UUIDs
- File extensions are preserved
- Example: `550e8400-e29b-41d4-a716-446655440000.jpg`

## Storage Structure

Images are stored in DigitalOcean Spaces with the following structure:

```
your-bucket-name/
├── 550e8400-e29b-41d4-a716-446655440000.jpg
├── 6ba7b810-9dad-11d1-80b4-00c04fd430c8.png
└── ...
```

Example with `cute33` bucket:
```
cute33/
├── 550e8400-e29b-41d4-a716-446655440000.jpg
├── 6ba7b810-9dad-11d1-80b4-00c04fd430c8.png
└── ...
```

## Security Considerations

### File Upload Security
- File type validation based on MIME type
- File size restrictions prevent abuse
- Unique filename generation prevents conflicts
- No executable file uploads allowed

### Access Control
- Uploaded images are publicly readable
- No authentication required for image access
- Consider implementing access controls for sensitive content

### Storage Security
- Use dedicated DigitalOcean Spaces bucket
- Configure appropriate CORS policies
- Regularly audit access keys and permissions

## Error Handling

The API implements comprehensive error handling:

1. **Validation Errors**: Return 400 Bad Request with descriptive messages
2. **Upload Failures**: Return 500 Internal Server Error for infrastructure issues
3. **Missing Files**: Return 400 Bad Request for empty or missing file uploads
4. **Storage Errors**: Graceful handling of DigitalOcean Spaces connectivity issues

## Testing

### Manual Testing

1. **Valid Upload:**
```bash
curl -X POST "http://localhost:5001/api/memes/upload" \
  -F "imageFile=@test.jpg" \
  -F "keywords=test"
```

2. **Invalid File Format:**
```bash
curl -X POST "http://localhost:5001/api/memes/upload" \
  -F "imageFile=@document.pdf" \
  -F "keywords=test"
```

3. **Large File Test:**
```bash
# Create a file larger than 10MB and test the size limit
```

### Integration Testing

Consider implementing automated tests for:
- File upload validation
- Storage integration
- Error handling
- Cleanup operations

## Monitoring and Maintenance

### Logging
The service logs upload attempts and errors. Monitor logs for:
- Failed uploads
- Invalid file attempts
- Storage connectivity issues

### Storage Management
- Monitor DigitalOcean Spaces usage and costs
- Implement cleanup policies for orphaned files
- Regular backup considerations

### Performance
- Monitor upload response times
- Consider implementing image optimization
- Add CDN integration for better performance

## Migration from URL-based Storage

If migrating from URL-based meme storage:

1. Existing memes with external URLs remain unchanged
2. New uploads automatically use DigitalOcean Spaces
3. Consider batch migration tools for existing content
4. Update client applications to use the new upload endpoint

## Troubleshooting

### Common Issues

1. **"Image file is required" error:**
   - Ensure the form field name is exactly `imageFile`
   - Check Content-Type is `multipart/form-data`

2. **Upload fails silently:**
   - Verify DigitalOcean Spaces credentials
   - Check bucket exists and permissions are correct
   - Ensure network connectivity to DigitalOcean

3. **Images not accessible:**
   - Verify bucket is configured for public read access
   - Check the returned URL format
   - Confirm CORS settings if accessing from web applications

### Debug Steps

1. Check application logs for detailed error messages
2. Verify DigitalOcean Spaces configuration
3. Test credentials using DigitalOcean CLI tools
4. Validate file format and size before upload

## API Client Examples

### Python Example

```python
import requests

# Upload image
with open('image.jpg', 'rb') as f:
    files = {'imageFile': f}
    data = {'keywords': 'python,example,test'}
    
    response = requests.post(
        'http://localhost:5001/api/memes/upload',
        files=files,
        data=data
    )
    
    if response.status_code == 201:
        meme = response.json()
        print(f"Upload successful: {meme['imageUrl']}")
    else:
        print(f"Upload failed: {response.text}")
```

### Node.js Example

```javascript
const axios = require('axios');
const FormData = require('form-data');
const fs = require('fs');

async function uploadMeme(imagePath, keywords) {
    const form = new FormData();
    form.append('imageFile', fs.createReadStream(imagePath));
    form.append('keywords', keywords);

    try {
        const response = await axios.post(
            'http://localhost:5001/api/memes/upload',
            form,
            { headers: form.getHeaders() }
        );
        
        console.log('Upload successful:', response.data);
        return response.data;
    } catch (error) {
        console.error('Upload failed:', error.response?.data || error.message);
        throw error;
    }
}

// Usage
uploadMeme('./image.jpg', 'nodejs,example,test');
```

This completes the comprehensive documentation for the image upload functionality.