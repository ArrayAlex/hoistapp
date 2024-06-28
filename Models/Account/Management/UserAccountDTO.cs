namespace hoistmt.Models.Account.Management;

public class UserAccountDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Contact { get; set; }
    public string Email { get; set; }
    public bool Active { get; set; }
    public string Username { get; set; }
    public string RoleName { get; set; }
    public string Position { get; set; }
    public string Phone { get; set; }
    public int RoleID { get; set; }
}
