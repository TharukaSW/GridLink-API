namespace GridLink.Models
{
    public class UserStatus
    {
        public const string Active = "Active";
        public const string PendingActivation = "PendingActivation";
        public const string Deactivated = "Deactivated";
        public const string DeactivationRequested = "DeactivationRequested";
        
        public static readonly IReadOnlyList<string> ValidStatus = new List<string>
        {
            Active,
            PendingActivation,
            Deactivated,
            DeactivationRequested
        };
        

    }
}
