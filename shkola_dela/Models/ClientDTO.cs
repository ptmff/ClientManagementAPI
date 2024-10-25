namespace shkola_dela.Models;

public class ClientDTO
{
    public int Id { get; set; }
    public string Inn { get; set; }
    public string Name { get; set; }
    public string Type { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }
    public List<int> FounderIds { get; set; } = new List<int>();
}