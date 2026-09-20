namespace GridLink.Models
{
    public class UserRole
    {
        public const string GridOperator = "GridOperator";
        public const string BackOffice = "BackOffice";
        public const string Prosumer = "Prosumer";

        public static readonly IReadOnlyList<string> AllRoles = new[] { BackOffice, GridOperator, Prosumer };
    }
}
