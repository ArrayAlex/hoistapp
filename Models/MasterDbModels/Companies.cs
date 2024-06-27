using System.ComponentModel.DataAnnotations;

namespace hoistmt.Models.MasterDbModels;

public class Companies
{
    [Key]
    public int _id { get; set; }
    public string CompanyID { get; set; }
    public int Credits { get; set; }
    
}