namespace hoistmt.Exceptions;

public class UnauthorizedException : Exception
{
    private static readonly string DefaultMessage = "You are not authorized to perform this action.";
    
    public UnauthorizedException() : base(DefaultMessage) { }
    public UnauthorizedException(string message) : base(message) { }
}