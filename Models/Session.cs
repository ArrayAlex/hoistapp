namespace hoistmt.Models;

public class Session
{
    public int id { get; set; }
    //id, userID, token, ipAddress, ExpiresAt, CompanyId, CompanyEmail, CompanyName, CompanyDb
    public int userID { get; set; }
    public string token { get; set; }
    public string ipAddress { get; set; }
    //expires in 1 hour
    public DateTime ExpiresAt { get; set; } = DateTime.Now.AddHours(1);
    public int? CompanyId { get; set; }
    public string? CompanyEmail { get; set; }
    public string? CompanyName { get; set; }
    public string CompanyDb { get; set; }
    
}