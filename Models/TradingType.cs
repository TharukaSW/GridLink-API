namespace GridLink.Models
{
    public class TradingType
    {
        public const string DropOff = "DropOff";
        public const string Charging = "Charging";

        public static readonly IReadOnlyList<string> AllTradingTypes = new[]
        {
            DropOff,
            Charging
        };
    }
}
