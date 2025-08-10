using MemeService.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MemeService.Controllers;

using Microsoft.AspNetCore.Mvc;
using MemeService.Models;

[ApiController]
[Route("api/[controller]")]
public class MemesController : ControllerBase
{
    private readonly MemeContext _context;
    public MemesController(MemeContext context) => _context = context;

    // GET: api/memes
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Meme>>> GetMemes() =>
        await _context.Memes.OrderByDescending(m => m.CreatedAt).ToListAsync();

    // GET: api/memes/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Meme>> GetMeme(int id)
    {
        var meme = await _context.Memes.FindAsync(id);
        return meme == null ? NotFound() : meme;
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
        _context.Memes.Remove(meme);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}