using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using shkola_dela.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Swashbuckle.AspNetCore.Filters;

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
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ClientGET.ClientResponseListExample))]
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
    [SwaggerResponseExample(StatusCodes.Status200OK, typeof(ClientGETID.ClientResponseExample))]
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
[SwaggerRequestExample(typeof(ClientPOST.ClientExample), typeof(ClientPOST.CreateClientRequestExample))]
[SwaggerResponseExample(StatusCodes.Status201Created, typeof(ClientPOST.CreateClientResponseExample))]
[ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ClientPOST.CreateClientResponse))]
public async Task<ActionResult<ClientDTO>> PostClient(ClientDTO clientDto)
{
    if (!Enum.TryParse(clientDto.Type, out ClientType clientType))
    {
        return BadRequest("Invalid client type. Valid values are 'IndividualEnterpreneur' or 'LegalPerson'.");
    }

    // Проверка длины ИНН в зависимости от типа клиента
    if ((clientType == ClientType.IndividualEnterpreneur && clientDto.Inn.Length != 12) ||
        (clientType == ClientType.LegalPerson && clientDto.Inn.Length != 10))
    {
        return BadRequest($"Invalid Inn length. For {clientType}, the length should be {(clientType == ClientType.IndividualEnterpreneur ? 12 : 10)}.");
    }

    // Проверка количества учредителей для IndividualEnterpreneur
    if (clientType == ClientType.IndividualEnterpreneur && clientDto.FounderIds.Count > 1)
    {
        return BadRequest("IndividualEnterpreneur can have at most one founder.");
    }

    using (var transaction = await _context.Database.BeginTransactionAsync())
    {
        try
        {
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
                foreach (var founderId in clientDto.FounderIds)
                {
                    var founder = await _context.Founders.FindAsync(founderId);
                    if (founder == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound($"Founder with ID {founderId} not found.");
                    }

                    var clientFounder = new ClientFounder
                    {
                        ClientId = client.Id,
                        FounderId = founderId
                    };

                    _context.ClientFounders.Add(clientFounder);
                }

                await _context.SaveChangesAsync();
            }

            await transaction.CommitAsync();

            clientDto.Id = client.Id;
            clientDto.DateAdded = client.DateAdded;
            clientDto.DateUpdated = client.DateUpdated;

            return CreatedAtAction(nameof(GetClient), new { id = client.Id }, clientDto);
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while creating the client.");
        }
    }
}



    
    // Обновление клиента
[HttpPut("{id}")]
[SwaggerRequestExample(typeof(ClientPOST.ClientExample), typeof(ClientGETID.ClientResponseExample))]
[ProducesResponseType(StatusCodes.Status204NoContent)]
public async Task<IActionResult> PutClient(int id, ClientDTO clientDto)
{
    if (id != clientDto.Id)
    {
        return BadRequest("Client ID mismatch.");
    }

    if (!Enum.TryParse(clientDto.Type, out ClientType clientType))
    {
        return BadRequest("Invalid client type. Valid values are 'IndividualEnterpreneur' or 'LegalPerson'.");
    }

    // Проверка длины ИНН в зависимости от типа клиента
    if ((clientType == ClientType.IndividualEnterpreneur && clientDto.Inn.Length != 12) ||
        (clientType == ClientType.LegalPerson && clientDto.Inn.Length != 10))
    {
        return BadRequest($"Invalid Inn length. For {clientType}, the length should be {(clientType == ClientType.IndividualEnterpreneur ? 12 : 10)}.");
    }

    // Проверка количества учредителей для IndividualEnterpreneur
    if (clientType == ClientType.IndividualEnterpreneur && clientDto.FounderIds.Count > 1)
    {
        return BadRequest("IndividualEnterpreneur can have at most one founder.");
    }

    var client = await _context.Clients.Include(c => c.ClientFounders).FirstOrDefaultAsync(c => c.Id == id);
    if (client == null)
    {
        return NotFound($"Client with ID {id} not found.");
    }

    using (var transaction = await _context.Database.BeginTransactionAsync())
    {
        try
        {
            client.Inn = clientDto.Inn;
            client.Name = clientDto.Name;
            client.Type = clientType;

            _context.Entry(client).State = EntityState.Modified;

            // Удаление текущих связей с учредителями
            var existingFounders = client.ClientFounders.ToList();
            _context.ClientFounders.RemoveRange(existingFounders);

            // Добавление новых учредителей
            if (clientDto.FounderIds != null && clientDto.FounderIds.Any())
            {
                foreach (var founderId in clientDto.FounderIds)
                {
                    var founder = await _context.Founders.FindAsync(founderId);
                    if (founder == null)
                    {
                        await transaction.RollbackAsync();
                        return NotFound($"Founder with ID {founderId} not found.");
                    }

                    var clientFounder = new ClientFounder
                    {
                        ClientId = client.Id,
                        FounderId = founderId
                    };

                    _context.ClientFounders.Add(clientFounder);
                }
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return NoContent();
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return StatusCode(500, "An error occurred while updating the client.");
        }
    }
}




    // Удаление клиента
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
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

