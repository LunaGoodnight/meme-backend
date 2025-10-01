# Next.js Frontend Image Upload Examples

This document provides complete examples for implementing image upload functionality in Next.js to work with the Meme Service API.

## 1. TypeScript Types

First, create the TypeScript types for your API responses:

```typescript
// types/meme.ts
export interface Meme {
  id: number;
  imageUrl: string;
  createdAt: string;
  keywords: string[];
}

export interface MemeUploadResponse {
  id: number;
  imageUrl: string;
  createdAt: string;
  keywords: string[];
}

export interface ApiError {
  error: string;
}
```

## 2. API Service Helper

Create a service to handle API calls:

```typescript
// services/memeService.ts
import { Meme, MemeUploadResponse } from '@/types/meme';

const API_BASE_URL = process.env.NEXT_PUBLIC_API_URL || 'http://localhost:5001/api';

export class MemeService {
  static async uploadMeme(imageFile: File, keywords: string = ''): Promise<MemeUploadResponse> {
    const formData = new FormData();
    formData.append('imageFile', imageFile);
    if (keywords.trim()) {
      formData.append('keywords', keywords.trim());
    }

    const response = await fetch(`${API_BASE_URL}/memes/upload`, {
      method: 'POST',
      body: formData,
    });

    if (!response.ok) {
      const errorData = await response.json().catch(() => ({ error: 'Upload failed' }));
      throw new Error(errorData.error || `HTTP error! status: ${response.status}`);
    }

    return response.json();
  }

  static async getAllMemes(): Promise<Meme[]> {
    const response = await fetch(`${API_BASE_URL}/memes`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return response.json();
  }

  static async deleteMeme(id: number): Promise<void> {
    const response = await fetch(`${API_BASE_URL}/memes/${id}`, {
      method: 'DELETE',
    });

    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
  }
}
```

## 3. Image Upload Component

Create a reusable image upload component:

```tsx
// components/MemeUploader.tsx
'use client';

import React, { useState, useRef } from 'react';
import { MemeService } from '@/services/memeService';
import { MemeUploadResponse } from '@/types/meme';

interface MemeUploaderProps {
  onUploadSuccess?: (meme: MemeUploadResponse) => void;
  onUploadError?: (error: string) => void;
}

export default function MemeUploader({ onUploadSuccess, onUploadError }: MemeUploaderProps) {
  const [isUploading, setIsUploading] = useState(false);
  const [previewUrl, setPreviewUrl] = useState<string | null>(null);
  const [keywords, setKeywords] = useState('');
  const [dragActive, setDragActive] = useState(false);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleFileSelect = (file: File) => {
    if (!file) return;

    // Validate file type
    const allowedTypes = ['image/jpeg', 'image/png', 'image/gif', 'image/webp'];
    if (!allowedTypes.includes(file.type)) {
      onUploadError?.('Only JPEG, PNG, GIF, and WebP images are allowed');
      return;
    }

    // Validate file size (10MB)
    if (file.size > 10 * 1024 * 1024) {
      onUploadError?.('Image size cannot exceed 10MB');
      return;
    }

    // Create preview
    const reader = new FileReader();
    reader.onload = (e) => {
      setPreviewUrl(e.target?.result as string);
    };
    reader.readAsDataURL(file);
  };

  const handleFileInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const file = e.target.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  const handleDrag = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    if (e.type === 'dragenter' || e.type === 'dragover') {
      setDragActive(true);
    } else if (e.type === 'dragleave') {
      setDragActive(false);
    }
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    e.stopPropagation();
    setDragActive(false);

    const file = e.dataTransfer.files?.[0];
    if (file) {
      handleFileSelect(file);
    }
  };

  const handleUpload = async () => {
    if (!fileInputRef.current?.files?.[0]) {
      onUploadError?.('Please select an image file');
      return;
    }

    setIsUploading(true);
    try {
      const file = fileInputRef.current.files[0];
      const result = await MemeService.uploadMeme(file, keywords);
      onUploadSuccess?.(result);
      
      // Reset form
      setPreviewUrl(null);
      setKeywords('');
      if (fileInputRef.current) {
        fileInputRef.current.value = '';
      }
    } catch (error) {
      onUploadError?.(error instanceof Error ? error.message : 'Upload failed');
    } finally {
      setIsUploading(false);
    }
  };

  const clearPreview = () => {
    setPreviewUrl(null);
    setKeywords('');
    if (fileInputRef.current) {
      fileInputRef.current.value = '';
    }
  };

  return (
    <div className="w-full max-w-md mx-auto p-6 bg-white rounded-lg shadow-md">
      <h2 className="text-2xl font-bold text-center mb-6">Upload Meme</h2>
      
      {/* File Drop Zone */}
      <div
        className={`relative border-2 border-dashed rounded-lg p-6 text-center transition-colors
          ${dragActive ? 'border-blue-500 bg-blue-50' : 'border-gray-300'}
          ${previewUrl ? 'hidden' : 'block'}
        `}
        onDragEnter={handleDrag}
        onDragLeave={handleDrag}
        onDragOver={handleDrag}
        onDrop={handleDrop}
      >
        <input
          ref={fileInputRef}
          type="file"
          accept="image/*"
          onChange={handleFileInputChange}
          className="absolute inset-0 w-full h-full opacity-0 cursor-pointer"
        />
        <div className="space-y-2">
          <div className="text-gray-500">
            <svg className="mx-auto h-12 w-12" stroke="currentColor" fill="none" viewBox="0 0 48 48">
              <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
            </svg>
          </div>
          <div className="text-sm text-gray-600">
            <span className="font-semibold">Click to upload</span> or drag and drop
          </div>
          <div className="text-xs text-gray-500">
            PNG, JPG, GIF, WebP up to 10MB
          </div>
        </div>
      </div>

      {/* Image Preview */}
      {previewUrl && (
        <div className="space-y-4">
          <div className="relative">
            <img
              src={previewUrl}
              alt="Preview"
              className="w-full h-48 object-cover rounded-lg"
            />
            <button
              onClick={clearPreview}
              className="absolute top-2 right-2 bg-red-500 text-white rounded-full w-6 h-6 flex items-center justify-center text-sm hover:bg-red-600"
            >
              ×
            </button>
          </div>

          {/* Keywords Input */}
          <div>
            <label htmlFor="keywords" className="block text-sm font-medium text-gray-700 mb-1">
              Keywords (optional)
            </label>
            <input
              id="keywords"
              type="text"
              value={keywords}
              onChange={(e) => setKeywords(e.target.value)}
              placeholder="funny, cat, meme (comma-separated, max 10)"
              className="w-full px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:ring-blue-500"
            />
            <p className="text-xs text-gray-500 mt-1">
              Separate keywords with commas (maximum 10)
            </p>
          </div>

          {/* Upload Button */}
          <button
            onClick={handleUpload}
            disabled={isUploading}
            className="w-full bg-blue-500 text-white py-2 px-4 rounded-md hover:bg-blue-600 disabled:opacity-50 disabled:cursor-not-allowed"
          >
            {isUploading ? (
              <div className="flex items-center justify-center">
                <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white mr-2"></div>
                Uploading...
              </div>
            ) : (
              'Upload Meme'
            )}
          </button>
        </div>
      )}
    </div>
  );
}
```

## 4. Meme Display Component

Create a component to display uploaded memes:

```tsx
// components/MemeCard.tsx
'use client';

import React, { useState } from 'react';
import { Meme } from '@/types/meme';
import { MemeService } from '@/services/memeService';

interface MemeCardProps {
  meme: Meme;
  onDelete?: (id: number) => void;
}

export default function MemeCard({ meme, onDelete }: MemeCardProps) {
  const [isDeleting, setIsDeleting] = useState(false);
  const [imageError, setImageError] = useState(false);

  const handleDelete = async () => {
    if (!confirm('Are you sure you want to delete this meme?')) return;

    setIsDeleting(true);
    try {
      await MemeService.deleteMeme(meme.id);
      onDelete?.(meme.id);
    } catch (error) {
      console.error('Delete failed:', error);
      alert('Failed to delete meme');
    } finally {
      setIsDeleting(false);
    }
  };

  const copyImageUrl = () => {
    navigator.clipboard.writeText(meme.imageUrl);
    alert('Image URL copied to clipboard!');
  };

  return (
    <div className="bg-white rounded-lg shadow-md overflow-hidden">
      {/* Image */}
      <div className="relative aspect-square">
        {!imageError ? (
          <img
            src={meme.imageUrl}
            alt="Meme"
            className="w-full h-full object-cover"
            onError={() => setImageError(true)}
          />
        ) : (
          <div className="w-full h-full bg-gray-200 flex items-center justify-center">
            <span className="text-gray-500">Image failed to load</span>
          </div>
        )}
        
        {/* Action buttons overlay */}
        <div className="absolute top-2 right-2 space-x-1">
          <button
            onClick={copyImageUrl}
            className="bg-black bg-opacity-50 text-white p-1 rounded hover:bg-opacity-70"
            title="Copy image URL"
          >
            <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
              <path d="M8 3a1 1 0 011-1h2a1 1 0 110 2H9a1 1 0 01-1-1z"></path>
              <path d="M6 3a2 2 0 00-2 2v11a2 2 0 002 2h8a2 2 0 002-2V5a2 2 0 00-2-2 3 3 0 01-3 3H9a3 3 0 01-3-3z"></path>
            </svg>
          </button>
          <button
            onClick={handleDelete}
            disabled={isDeleting}
            className="bg-red-500 text-white p-1 rounded hover:bg-red-600 disabled:opacity-50"
            title="Delete meme"
          >
            {isDeleting ? (
              <div className="animate-spin rounded-full h-4 w-4 border-b-2 border-white"></div>
            ) : (
              <svg className="w-4 h-4" fill="currentColor" viewBox="0 0 20 20">
                <path fillRule="evenodd" d="M9 2a1 1 0 00-.894.553L7.382 4H4a1 1 0 000 2v10a2 2 0 002 2h8a2 2 0 002-2V6a1 1 0 100-2h-3.382l-.724-1.447A1 1 0 0011 2H9zM7 8a1 1 0 012 0v6a1 1 0 11-2 0V8zm5-1a1 1 0 00-1 1v6a1 1 0 102 0V8a1 1 0 00-1-1z" clipRule="evenodd"></path>
              </svg>
            )}
          </button>
        </div>
      </div>

      {/* Content */}
      <div className="p-4">
        {/* Keywords */}
        {meme.keywords && meme.keywords.length > 0 && (
          <div className="mb-2">
            <div className="flex flex-wrap gap-1">
              {meme.keywords.map((keyword, index) => (
                <span
                  key={index}
                  className="inline-block bg-blue-100 text-blue-800 text-xs px-2 py-1 rounded-full"
                >
                  #{keyword}
                </span>
              ))}
            </div>
          </div>
        )}

        {/* Created date */}
        <p className="text-sm text-gray-500">
          {new Date(meme.createdAt).toLocaleDateString('en-US', {
            year: 'numeric',
            month: 'short',
            day: 'numeric',
            hour: '2-digit',
            minute: '2-digit'
          })}
        </p>
      </div>
    </div>
  );
}
```

## 5. Complete Page Example

Create a complete page that combines upload and display functionality:

```tsx
// app/page.tsx or pages/index.tsx (depending on your Next.js version)
'use client';

import React, { useState, useEffect } from 'react';
import MemeUploader from '@/components/MemeUploader';
import MemeCard from '@/components/MemeCard';
import { MemeService } from '@/services/memeService';
import { Meme, MemeUploadResponse } from '@/types/meme';

export default function HomePage() {
  const [memes, setMemes] = useState<Meme[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  // Load memes on component mount
  useEffect(() => {
    loadMemes();
  }, []);

  const loadMemes = async () => {
    try {
      setIsLoading(true);
      const memesData = await MemeService.getAllMemes();
      setMemes(memesData);
      setError(null);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'Failed to load memes');
    } finally {
      setIsLoading(false);
    }
  };

  const handleUploadSuccess = (newMeme: MemeUploadResponse) => {
    setMemes(prev => [newMeme, ...prev]);
    setSuccessMessage('Meme uploaded successfully!');
    setTimeout(() => setSuccessMessage(null), 3000);
  };

  const handleUploadError = (errorMessage: string) => {
    setError(errorMessage);
    setTimeout(() => setError(null), 5000);
  };

  const handleMemeDelete = (deletedId: number) => {
    setMemes(prev => prev.filter(meme => meme.id !== deletedId));
    setSuccessMessage('Meme deleted successfully!');
    setTimeout(() => setSuccessMessage(null), 3000);
  };

  return (
    <div className="min-h-screen bg-gray-100">
      <div className="container mx-auto px-4 py-8">
        <h1 className="text-4xl font-bold text-center mb-8 text-gray-800">
          Meme Gallery
        </h1>

        {/* Success/Error Messages */}
        {successMessage && (
          <div className="mb-6 max-w-md mx-auto">
            <div className="bg-green-100 border border-green-400 text-green-700 px-4 py-3 rounded">
              {successMessage}
            </div>
          </div>
        )}

        {error && (
          <div className="mb-6 max-w-md mx-auto">
            <div className="bg-red-100 border border-red-400 text-red-700 px-4 py-3 rounded">
              {error}
            </div>
          </div>
        )}

        {/* Upload Section */}
        <div className="mb-12">
          <MemeUploader
            onUploadSuccess={handleUploadSuccess}
            onUploadError={handleUploadError}
          />
        </div>

        {/* Gallery Section */}
        <div className="max-w-6xl mx-auto">
          <h2 className="text-2xl font-semibold mb-6 text-gray-800">
            All Memes ({memes.length})
          </h2>

          {isLoading ? (
            <div className="flex justify-center items-center py-12">
              <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-500"></div>
              <span className="ml-3 text-gray-600">Loading memes...</span>
            </div>
          ) : memes.length === 0 ? (
            <div className="text-center py-12">
              <div className="text-gray-500 text-lg">
                No memes uploaded yet. Upload your first meme above!
              </div>
            </div>
          ) : (
            <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
              {memes.map((meme) => (
                <MemeCard
                  key={meme.id}
                  meme={meme}
                  onDelete={handleMemeDelete}
                />
              ))}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
```

## 6. Environment Configuration

Create a `.env.local` file for your Next.js project:

```bash
# .env.local
NEXT_PUBLIC_API_URL=http://localhost:5001/api
```

## 7. Tailwind CSS Setup

If you haven't set up Tailwind CSS yet, install it:

```bash
npm install -D tailwindcss postcss autoprefixer
npx tailwindcss init -p
```

Update your `tailwind.config.js`:

```javascript
/** @type {import('tailwindcss').Config} */
module.exports = {
  content: [
    './pages/**/*.{js,ts,jsx,tsx,mdx}',
    './components/**/*.{js,ts,jsx,tsx,mdx}',
    './app/**/*.{js,ts,jsx,tsx,mdx}',
  ],
  theme: {
    extend: {},
  },
  plugins: [],
}
```

## 8. Package.json Dependencies

Make sure you have these dependencies:

```json
{
  "dependencies": {
    "next": "latest",
    "react": "latest",
    "react-dom": "latest"
  },
  "devDependencies": {
    "@types/node": "latest",
    "@types/react": "latest",
    "@types/react-dom": "latest",
    "autoprefixer": "latest",
    "postcss": "latest",
    "tailwindcss": "latest",
    "typescript": "latest"
  }
}
```

## Key Features Implemented:

1. **Drag & Drop Upload**: Users can drag images directly onto the upload area
2. **Image Preview**: Shows preview before upload with ability to cancel
3. **File Validation**: Checks file type and size before upload
4. **Keywords Support**: Optional keywords input with validation
5. **Error Handling**: Comprehensive error handling with user feedback
6. **Loading States**: Visual feedback during upload and loading operations
7. **Responsive Design**: Works on mobile and desktop
8. **Image Gallery**: Displays all uploaded memes in a responsive grid
9. **Delete Functionality**: Users can delete memes with confirmation
10. **URL Copying**: Easy copying of image URLs to clipboard

## Usage:

1. Set up your Next.js project with the above files
2. Configure your API URL in `.env.local`
3. Install dependencies and run `npm run dev`
4. The app will provide a complete meme upload and gallery experience

This provides a production-ready frontend for your meme upload API with excellent user experience and error handling.