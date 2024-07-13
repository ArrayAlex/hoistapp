namespace hoistmt.Models;

public class UserAccount
{
    //ID, Name, Password, contact, email, Active, Username
    public int Id { get; set; }
    public string Name { get; set; }
    public string Password { get; set; }
    public string? contact { get; set; }
    public string email { get; set; }
    public bool Active { get; set; }
    public string Username { get; set; }

    public string? roleName { get; set; }

    public string? position { get; set; }

    public string? phone { get; set; }

    public int roleID { get; set; }
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    public string? VerificationToken { get; set; }
    public DateTime? VerificationTokenExpiry { get; set; }
    public bool? IsVerified { get; set; }
    
}