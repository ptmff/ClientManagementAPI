namespace shkola_dela.Models;

public class ClientFounder
{
    public int ClientId { get; set; }
    public Client Client { get; set; }

    public int FounderId { get; set; }
    public Founder Founder { get; set; }
}