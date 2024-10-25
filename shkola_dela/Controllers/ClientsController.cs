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
}

