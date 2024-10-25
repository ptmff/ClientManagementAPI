namespace shkola_dela.Models;

using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

public class Client
{
    public int Id { get; set; }

    [Required]
    [StringLength(12, MinimumLength = 10)]
    public string Inn { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    public string Type { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;

    public ICollection<ClientFounder> ClientFounders { get; set; } = new List<ClientFounder>();
}