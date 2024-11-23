
namespace hoistmt.Models;

public class Customer
{
    public int id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; } // Correct casing
    public string Email { get; set; } // Correct casing
    public string? Phone { get; set; } // Correct casing
    public DateTime? DOB { get; set; } // Date of Birth
    public DateTime? created_at { get; set; } // Date when the customer was created
    public string? postal_address { get; set; } 
    // public string AccountType { get; set; } 
    // public bool? AccountApproved { get; set; } 
    // public bool? OnHold { get; set; } 
    public string? notes { get; set; } 
    public DateTime? modified_at { get; set; }
}