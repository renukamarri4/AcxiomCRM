using System.ComponentModel.DataAnnotations;

namespace AcxiomCRM.Models;

public class FollowUp
{
    public int Id { get; set; }

    [Required]
    public string CustomerName { get; set; } = string.Empty;

    [Required]
    public string Notes { get; set; } = string.Empty;

    public DateTime FollowUpDate { get; set; } = DateTime.Today;

    public string Status { get; set; } = "Pending";
}