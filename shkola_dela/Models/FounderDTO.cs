namespace shkola_dela.Models;

public class FounderDTO
{
    public int Id { get; set; }
    public string Inn { get; set; }
    public string FullName { get; set; }
    public DateTime DateAdded { get; set; }
    public DateTime DateUpdated { get; set; }
    public List<int> ClientIds { get; set; } = new List<int>();
}