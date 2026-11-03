
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AcxiomCRM.Models;

public class Lead
{
    public int Id { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Phone]
    public string Phone { get; set; } = string.Empty;

    [StringLength(150)]
    public string Company { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "New";

    [Required]
    public string Source { get; set; } = "Website";

    [Required]
    public string Priority { get; set; } = "Medium";

    // User assigned to manage this lead
    public string? AssignedUserId { get; set; }

    public IdentityUser? AssignedUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
