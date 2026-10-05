namespace GridLink.Models
{
    public class JwtSettings
    {
        public String Secret { get; set; } = "GridLink_Smart_Solar_Microgrid_Trading_Secret_Key_2026_Secure_JWT_Key!"; 
        public String Issuer { get; set; } = string.Empty;
        public String Audience { get; set; } = string.Empty;
        public int DurationInMinutes { get; set; } = 1440;
    }
}
