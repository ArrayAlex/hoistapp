using hoistmt.Models.httpModels;

namespace hoistmt.HttpClients;

public class Result
{
    public RegoData Data { get; set; }
    public string Error { get; set; }
}