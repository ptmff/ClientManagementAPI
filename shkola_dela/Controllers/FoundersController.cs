using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shkola_dela.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class FoundersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public FoundersController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получение всех учредителей
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FounderDTO>>> GetFounders()
    {
        var founders = await _context.Founders
            .Include(f => f.ClientFounders)
            .Select(f => new FounderDTO
            {
                Id = f.Id,
                Inn = f.Inn,
                FullName = f.FullName,
                DateAdded = f.DateAdded,
                DateUpdated = f.DateUpdated,
                ClientIds = f.ClientFounders.Select(cf => cf.ClientId).ToList()
            })
            .ToListAsync();

        return founders;
    }

    // Получение учредителя по ID
    [HttpGet("{id}")]
    public async Task<ActionResult<FounderDTO>> GetFounder(int id)
    {
        var founder = await _context.Founders
            .Include(f => f.ClientFounders)
            .Where(f => f.Id == id)
            .Select(f => new FounderDTO
            {
                Id = f.Id,
                Inn = f.Inn,
                FullName = f.FullName,
                DateAdded = f.DateAdded,
                DateUpdated = f.DateUpdated,
                ClientIds = f.ClientFounders.Select(cf => cf.ClientId).ToList()
            })
            .FirstOrDefaultAsync();

        if (founder == null)
        {
            return NotFound();
        }

        return founder;
    }

    // Создание учредителя
    [HttpPost]
    public async Task<ActionResult<FounderDTO>> PostFounder(FounderDTO founderDto)
    {
        var founder = new Founder
        {
            Inn = founderDto.Inn,
            FullName = founderDto.FullName
        };

        _context.Founders.Add(founder);
        await _context.SaveChangesAsync();

        if (founderDto.ClientIds != null && founderDto.ClientIds.Any())
        {
            var clientFounders = founderDto.ClientIds.Select(clientId => new ClientFounder
            {
                ClientId = clientId,
                FounderId = founder.Id
            }).ToList();

            _context.ClientFounders.AddRange(clientFounders);
            await _context.SaveChangesAsync();
        }

        founderDto.Id = founder.Id;
        founderDto.DateAdded = founder.DateAdded;
        founderDto.DateUpdated = founder.DateUpdated;

        return CreatedAtAction(nameof(GetFounder), new { id = founder.Id }, founderDto);
    }
}