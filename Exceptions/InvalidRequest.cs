namespace hoistmt.Services;

public class InvalidRequest : Exception
{
    public InvalidRequest(string message) : base(message)
    {
    }
}