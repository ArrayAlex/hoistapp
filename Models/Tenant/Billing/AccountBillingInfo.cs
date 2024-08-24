using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models.Billing;

public class AccountBillingInfo
{
// {
//         CompanyId INT AUTO_INCREMENT PRIMARY KEY, -- Unique identifier for each company record
//     BusinessName NVARCHAR(255) NOT NULL,      -- Name of the business
//     AddressLine1 NVARCHAR(255) NOT NULL,      -- First line of the business address
//     AddressLine2 NVARCHAR(255) NULL,          -- Second line of the business address (optional)
//     City NVARCHAR(100) NOT NULL,              -- City where the business is located
//     PostalCode NVARCHAR(20) NOT NULL,         -- Postal or ZIP code
//     Country NVARCHAR(100) NOT NULL,           -- Country of the business
//     PhoneNumber NVARCHAR(20) NULL,            -- Contact phone number (optional)
//     Email NVARCHAR(255) NULL,                 -- Contact email address (optional)
//     Website NVARCHAR(255) NULL,               -- Business website (optional)
//     BillingContact NVARCHAR(255) NULL,        -- Contact person for billing (optional)
//     CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,  -- Timestamp of when the record was created
//     UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP, -- Timestamp of when the record was last updated
//     IsActive BIT DEFAULT 1 \

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
