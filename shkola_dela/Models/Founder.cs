namespace shkola_dela.Models;

using System.ComponentModel.DataAnnotations;

public class Founder
{
    public int Id { get; set; }

    [Required]
    [StringLength(12, MinimumLength = 12)] // ИНН учредителя всегда 12 цифр
    public string Inn { get; set; }

    [Required]
    [MaxLength(255)]
    public string FullName { get; set; }

    public DateTime DateAdded { get; set; } = DateTime.UtcNow;

    public DateTime DateUpdated { get; set; } = DateTime.UtcNow;

    public int ClientId { get; set; }

    public Client Client { get; set; }
}
