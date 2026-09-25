namespace GridLink.Models
{
    public class ReservationStatus
    {
        public const string Pending = "Pending";
        public const string Approved = "Approved";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
        public const string Rejected = "Rejected";

        public static readonly IReadOnlyList<string> AllStatuses = new[]
        {
            Pending,
            Approved,
            Completed,
            Cancelled,
            Rejected
        };
    }
}
