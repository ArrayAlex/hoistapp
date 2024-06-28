using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models.Account;

public class ClientCharges
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string TenantId { get; set; } // ID of the tenant making the charge

    [Required]
    public string ClientId { get; set; } // ID of the client being charged

    [Required]
    public double Amount { get; set; } // Amount being charged

    [Required]
    public DateTime Date { get; set; } // Date of the charge

    [Required]
    public string Description { get; set; } // Description of the charge

    public string? InvoiceId { get; set; } // Optional ID of the associated invoice

    public string? Status { get; set; } // Status of the charge (e.g., Paid, Pending, Cancelled)

    // Optional fields for additional information
    public string? PaymentMethod { get; set; } // Method of payment used
    public DateTime? PaymentDate { get; set; } // Date of payment, if applicable
}
