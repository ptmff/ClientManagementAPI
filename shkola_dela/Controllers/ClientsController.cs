using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shkola_dela.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public ClientsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Получение всех клиентов
    [HttpGet]
    public async Task<ActionResult<IEnumerable<ClientDTO>>> GetClients()
    {
        var clients = await _context.Clients
            .Include(c => c.ClientFounders)
            .Select(c => new ClientDTO
            {
                Id = c.Id,
                Inn = c.Inn,
                Name = c.Name,
                Type = c.Type.ToString(),  // Преобразование ClientType в строку
                DateAdded = c.DateAdded,
                DateUpdated = c.DateUpdated,
                FounderIds = c.ClientFounders.Select(cf => cf.FounderId).ToList()
            })
            .ToListAsync();

        return clients;
    }

// Получение клиента по ID
    [HttpGet("{id}")]
    public async Task<ActionResult<ClientDTO>> GetClient(int id)
    {
        var client = await _context.Clients
            .Include(c => c.ClientFounders)
            .Where(c => c.Id == id)
            .Select(c => new ClientDTO
            {
                Id = c.Id,
                Inn = c.Inn,
                Name = c.Name,
                Type = c.Type.ToString(),  // Преобразование ClientType в строку
                DateAdded = c.DateAdded,
                DateUpdated = c.DateUpdated,
                FounderIds = c.ClientFounders.Select(cf => cf.FounderId).ToList()
            })
            .FirstOrDefaultAsync();

        if (client == null)
        {
            return NotFound();
        }

        return client;
    }


    // Создание клиента
    [HttpPost]
    public async Task<ActionResult<ClientDTO>> PostClient(ClientDTO clientDto)
    {
        if (!Enum.TryParse(clientDto.Type, out ClientType clientType))
        {
            return BadRequest("Invalid client type. Valid values are 'IndividualEnterpreneur' or 'LegalPerson'.");
        }

        var client = new Client
        {
            Inn = clientDto.Inn,
            Name = clientDto.Name,
            Type = clientType
        };

        _context.Clients.Add(client);
        await _context.SaveChangesAsync();

        if (clientDto.FounderIds != null && clientDto.FounderIds.Any())
        {
            var clientFounders = clientDto.FounderIds.Select(founderId => new ClientFounder
            {
                ClientId = client.Id,
                FounderId = founderId
            }).ToList();

            _context.ClientFounders.AddRange(clientFounders);
            await _context.SaveChangesAsync();
        }

        clientDto.Id = client.Id;
        clientDto.DateAdded = client.DateAdded;
        clientDto.DateUpdated = client.DateUpdated;

        return CreatedAtAction(nameof(GetClient), new { id = client.Id }, clientDto);
    }
    
    // Обновление информации о клиенте
    [HttpPut("{id}")]
    public async Task<IActionResult> PutClient(int id, ClientDTO clientDto)
    {
        if (id != clientDto.Id)
        {
            return BadRequest("Client ID mismatch.");
        }

        var client = await _context.Clients
            .Include(c => c.ClientFounders)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound();
        }

        // Обновляем поля клиента
        client.Inn = clientDto.Inn;
        client.Name = clientDto.Name;

        if (!Enum.TryParse(clientDto.Type, out ClientType clientType))
        {
            return BadRequest("Invalid client type. Valid values are 'IndividualEnterpreneur' or 'LegalPerson'.");
        }

        client.Type = clientType;

        // Обновление учредителей клиента (удаление старых и добавление новых)
        var existingClientFounders = client.ClientFounders.ToList();
        _context.ClientFounders.RemoveRange(existingClientFounders);

        if (clientDto.FounderIds != null && clientDto.FounderIds.Any())
        {
            var newClientFounders = clientDto.FounderIds.Select(founderId => new ClientFounder
            {
                ClientId = client.Id,
                FounderId = founderId
            }).ToList();

            _context.ClientFounders.AddRange(newClientFounders);
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    // Удаление клиента
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteClient(int id)
    {
        var client = await _context.Clients
            .Include(c => c.ClientFounders)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (client == null)
        {
            return NotFound();
        }

        // Удаляем записи о связях клиента с учредителями
        _context.ClientFounders.RemoveRange(client.ClientFounders);

        // Удаляем клиента
        _context.Clients.Remove(client);
    
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

