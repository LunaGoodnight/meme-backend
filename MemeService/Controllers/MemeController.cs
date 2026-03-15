using MemeService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeService.Controllers;

using Microsoft.AspNetCore.Mvc;
using MemeService.Models;
using MemeService.Services;

[ApiController]
[Route("api/[controller]")]
public class MemesController : ControllerBase
{
    private readonly MemeContext _context;
    private readonly IImageUploadService _imageUploadService;
    
    public MemesController(MemeContext context, IImageUploadService imageUploadService)
    {
        _context = context;
        _imageUploadService = imageUploadService;
    }

    // GET: api/memes
    [HttpGet]
    public async Task<ActionResult<object>> GetMemes([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        pageSize = Math.Clamp(pageSize, 1, 50);
        page = Math.Max(1, page);

        var totalCount = await _context.Memes.CountAsync();
        var memes = await _context.Memes
            .OrderByDescending(m => m.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new
        {
            data = memes,
            page,
            pageSize,
            totalCount,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
            hasMore = page * pageSize < totalCount
        };
    }

    // GET: api/memes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Meme>> GetMeme(int id)
    {
        var meme = await _context.Memes.FindAsync(id);
        return meme == null ? NotFound() : meme;
    }


    // POST: api/memes/upload
    [HttpPost("upload")]
    public async Task<ActionResult<Meme>> UploadMeme([FromForm] IFormFile imageFile, [FromForm] string keywords = "")
    {
        try
        {
            var imageUrl = await _imageUploadService.UploadImageAsync(imageFile);

            var meme = new Meme
            {
                ImageUrl = imageUrl,
                Keywords = string.IsNullOrEmpty(keywords)
                    ? new List<string>()
                    : keywords.Split(',').Select(k => k.Trim()).Take(10).ToList()
            };

            _context.Memes.Add(meme);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return StatusCode(500, "An error occurred while uploading the image");
        }
    }

    // POST: api/memes
    [HttpPost]
    public async Task<ActionResult<Meme>> CreateMeme(Meme meme)
    {
        _context.Memes.Add(meme);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetMeme), new { id = meme.Id }, meme);
    }

    // PUT: api/memes/5
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateMeme(int id, Meme updated)
    {
        if (id != updated.Id) return BadRequest();
        _context.Entry(updated).State = EntityState.Modified;
        await _context.SaveChangesAsync();
        return NoContent();
    }

    // DELETE: api/memes/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteMeme(int id)
    {
        var meme = await _context.Memes.FindAsync(id);
        if (meme == null) return NotFound();
        
        // Try to delete the image from storage
        await _imageUploadService.DeleteImageAsync(meme.ImageUrl);
        
        _context.Memes.Remove(meme);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}