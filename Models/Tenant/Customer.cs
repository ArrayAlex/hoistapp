
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
    public int? created_by { get; set; } 
    public int? updated_by { get; set; } 
    public DateTime? modified_at { get; set; }
    
}

public class CustomerWithAccountDetails
{
    public int id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string? Phone { get; set; }
    public DateTime? DOB { get; set; }
    public DateTime? CreatedAt { get; set; }
    public string? PostalAddress { get; set; }
    public string? Notes { get; set; }
    public DateTime? ModifiedAt { get; set; }
    public int? UpdatedBy { get; set; } // ID of the account that modified the customer
    public AccountDetails? AccountDetails { get; set; } // Account details of the user who modified the customer
}

public class AccountDetails
{
    public int ID { get; set; }
    public string Name { get; set; }

}