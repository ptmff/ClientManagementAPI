namespace shkola_dela.Controllers;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

[Route("api/[controller]")]
[ApiController]
public class FoundersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FoundersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/founders
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Founder>>> GetFounders()
    {
        return await _context.Founders.Include(f => f.Client).ToListAsync();
    }

    // GET: api/founders/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Founder>> GetFounder(int id)
    {
        var founder = await _context.Founders.Include(f => f.Client).FirstOrDefaultAsync(f => f.Id == id);
        if (founder == null)
        {
            return NotFound();
        }
        return founder;
    }

    // POST: api/founders
    [HttpPost]
    public async Task<ActionResult<Founder>> PostFounder(Founder founder)
    {
        _context.Founders.Add(founder);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFounder), new { id = founder.Id }, founder);
    }

    // PUT: api/founders/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFounder(int id, Founder founder)
    {
        if (id != founder.Id)
        {
            return BadRequest();
        }

        _context.Entry(founder).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    // DELETE: api/founders/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFounder(int id)
    {
        var founder = await _context.Founders.FindAsync(id);
        if (founder == null)
        {
            return NotFound();
        }

        _context.Founders.Remove(founder);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}
