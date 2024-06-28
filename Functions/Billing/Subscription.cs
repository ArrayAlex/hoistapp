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
    public Boolean AccessFeatureA { get; set; }
    public Boolean AccessFeatureB { get; set; }
    public Boolean AccessFeatureC { get; set; }
    public Boolean AccessFeatureD { get; set; }
    public Boolean AccessFeatureE { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}