
namespace hoistmt.Models;

public class Customer
{
    public int id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; } // Correct casing
    public string Email { get; set; } // Correct casing
    public string? Phone { get; set; } // Correct casing
    public DateTime? DOB { get; set; } // Date of Birth
    public DateTime? CreatedAt { get; set; } // Date when the customer was created
    public string? PostalAddress { get; set; } 
    // public string AccountType { get; set; } 
    // public bool? AccountApproved { get; set; } 
    // public bool? OnHold { get; set; } 
    public string? Notes { get; set; } 
    public DateTime? ModifiedAt { get; set; }
}