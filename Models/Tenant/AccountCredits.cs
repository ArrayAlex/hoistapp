namespace hoistmt.Models.Account;

public class CreditsDto
{
    private double _credits;

    public double Credits
    {
        get => Math.Round(_credits, 2);
        set => _credits = value;
    }
}
