namespace GridLink.Models
{
    public class JwtSettings
    {
        public String Secret { get; set; } = string.Empty; 
        public String Issuer { get; set; } = string.Empty;
        public String Audience { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; } = 1440;
    }
}
