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
    using (var transaction = await _context.Database.BeginTransactionAsync())
    {
        try
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
                foreach (var clientId in founderDto.ClientIds)
                {
                    var client = await _context.Clients.Include(c => c.ClientFounders).FirstOrDefaultAsync(c => c.Id == clientId);

                    if (client == null)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest($"Client with ID {clientId} not found.");
                    }

                    if (client.Type == ClientType.IndividualEnterpreneur && client.ClientFounders.Count >= 1)
                    {
                        await transaction.RollbackAsync();
                        return BadRequest("IndividualEnterpreneur can have at most one founder.");
                    }

                    var clientFounder = new ClientFounder
                    {
                        ClientId = clientId,
                        FounderId = founder.Id
                    };

                    _context.ClientFounders.Add(clientFounder);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            founderDto.Id = founder.Id;
            founderDto.DateAdded = founder.DateAdded;
            founderDto.DateUpdated = founder.DateUpdated;

            return CreatedAtAction(nameof(GetFounder), new { id = founder.Id }, founderDto);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while creating the founder.");
        }
    }
}


    
    // Обновление информации об учредителе
    [HttpPut("{id}")]
    public async Task<IActionResult> PutFounder(int id, FounderDTO founderDto)
    {
        if (id != founderDto.Id)
        {
            return BadRequest("Founder ID mismatch.");
        }

        var founder = await _context.Founders
            .Include(f => f.ClientFounders)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (founder == null)
        {
            return NotFound();
        }

        // Проверка на количество учредителей у IndividualEnterpreneur
        foreach (var clientId in founderDto.ClientIds)
        {
            var client = await _context.Clients.Include(c => c.ClientFounders).FirstOrDefaultAsync(c => c.Id == clientId);

            if (client == null)
            {
                return BadRequest($"Client with ID {clientId} not found.");
            }

            if (client.Type == ClientType.IndividualEnterpreneur && client.ClientFounders.Count >= 1)
            {
                return BadRequest("IndividualEnterpreneur can have at most one founder.");
            }
        }

        // Обновляем данные учредителя
        founder.Inn = founderDto.Inn;
        founder.FullName = founderDto.FullName;

        // Удаление старых и добавление новых связей с клиентами
        var existingClientFounders = founder.ClientFounders.ToList();
        _context.ClientFounders.RemoveRange(existingClientFounders);

        if (founderDto.ClientIds != null && founderDto.ClientIds.Any())
        {
            foreach (var clientId in founderDto.ClientIds)
            {
                var clientFounder = new ClientFounder
                {
                    ClientId = clientId,
                    FounderId = founder.Id
                };
                _context.ClientFounders.Add(clientFounder);
            }
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }


    // Удаление учредителя
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteFounder(int id)
    {
        var founder = await _context.Founders
            .Include(f => f.ClientFounders)
            .FirstOrDefaultAsync(f => f.Id == id);

        if (founder == null)
        {
            return NotFound();
        }

        // Удаляем записи о связях учредителя с клиентами
        _context.ClientFounders.RemoveRange(founder.ClientFounders);

        // Удаляем учредителя
        _context.Founders.Remove(founder);
    
        await _context.SaveChangesAsync();

        return NoContent();
    }
}