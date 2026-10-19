# Media uploads

`POST /api/memes/upload` accepts JPEG, PNG, GIF, WebP, and MP4 through the existing `imageFile` multipart field. The response continues to use `imageUrl` for both images and videos, so no database migration is required.

Each file is limited to 10 MiB. The frontend uploads MP4 bytes unchanged without image compression or transcoding. Playback depends on the browser supporting the video's codecs.

For Nginx, set `client_max_body_size 11m;` in the API server block to allow a 10 MiB file plus multipart overhead. Test with `nginx -t` and reload Nginx after updating the configuration.
