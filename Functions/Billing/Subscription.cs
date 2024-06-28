namespace hoistmt.Functions;

public class Subscription
{
    //id, PlanName, MonthlyCost, AnnualCost, MaxUsers, StorageLimitGB, SupportLevel, AccessFeatureA, AccessFeatureB, AccessFeatureC, AccessFeatureD, AccessFeatureE, CreatedAt, UpdatedAt
    public int id { get; set; }
    public string PlanName { get; set; }
    public decimal MonthlyCost { get; set; }
    public decimal AnnualCost { get; set; }
    public int MaxUsers { get; set; }
    public int StorageLimitGB { get; set; }
    public string SupportLevel { get; set; }
    public string AccessFeatureA { get; set; }
    public string AccessFeatureB { get; set; }
    public string AccessFeatureC { get; set; }
    public string AccessFeatureD { get; set; }
    public string AccessFeatureE { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}