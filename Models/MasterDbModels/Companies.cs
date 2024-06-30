using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models.MasterDbModels;

public class Companies
{
    [Key]
    public int _id { get; set; }
    public string CompanyID { get; set; }
    public double Credits { get; set; }
    public string PlanName { get; set; }
    public int PlanID { get; set; }
    public DateTime? PrevBilling { get; set; }
    public DateTime NextBilling { get; set; }
    
}