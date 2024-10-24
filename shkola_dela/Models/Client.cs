using shkola_dela.Models.Enums;

namespace shkola_dela.Models;

using System.ComponentModel.DataAnnotations;

public class Client
{
    public int Id { get; set; }

    [Required]
    [StringLength(12, MinimumLength = 10)] // ИНН может быть длиной 10 (ИП) или 12 (ЮЛ)
    public string Inn { get; set; }

    [Required]
    [MaxLength(255)]
    public string Name { get; set; }

    [Required]
    public ClientType Type { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;

    public ICollection<Founder>? Founders { get; set; } = new List<Founder>(); 
}
