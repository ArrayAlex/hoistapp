namespace hoistmt.Models;

public class Vehicle
{
    //id, clientID, make, model, year, owner, status, lastmodified, description, active
    public int id { get; set; }
    public int? customerid { get; set; }
    public string? owner { get; set; }
    public string make { get; set; }
    public string? description { get; set; }
    public string? model { get; set; }
    public string? rego { get; set; }
    public string? vin { get; set; }
    public int? year { get; set; }

}