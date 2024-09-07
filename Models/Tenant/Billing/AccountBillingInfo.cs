using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models.Billing;

public class AccountBillingInfo
{


    [Key]
    public int CompanyId { get; set; }
    public string BusinessName { get; set; }
    public string AddressLine1 { get; set; }
    public string AddressLine2 { get; set; }
    public string City { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
    public string PhoneNumber { get; set; }
    public string Email { get; set; }
    public string Website { get; set; }
    public string BillingContact { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
