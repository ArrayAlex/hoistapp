using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models.MasterDbModels;

public class TenantTransactions
{
    [Key]
    public int Id { get; set; }

    [Required]
    public string TenantId { get; set; } // ID of the tenant making the transaction

    [Required]
    public double Amount { get; set; } // Amount of the transaction

    [Required]
    public DateTime Date { get; set; } // Date of the transaction

    [Required]
    public string Description { get; set; } // Description of the transaction

    public string? TransactionType { get; set; } // Type of transaction (e.g., Payment, Refund)

    public string? Status { get; set; } // Status of the transaction (e.g., Completed, Pending, Failed)

    // Optional fields for additional information
    public string? PaymentMethod { get; set; } // Method of payment used
    public DateTime? PaymentDate { get; set; } // Date of payment, if applicable
}
