
using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models;

public class Opportunity
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string CustomerName { get; set; } = string.Empty;

    [Range(0, 100000000)]
    public decimal Amount { get; set; }

    [Required]
    public string Stage { get; set; } = "Prospecting";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
