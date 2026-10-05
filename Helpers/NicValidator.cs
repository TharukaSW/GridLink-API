using System.Text.RegularExpressions;

namespace GridLink.Helpers
{
    public static class NicValidator
    {
        public static string Normalize(string nic)
        {
            if(string.IsNullOrWhiteSpace(nic))
                return string.Empty;

            return nic.Trim().ToUpperInvariant();
            
        }

        public static bool IsValid(string nic)
        {
            if (string.IsNullOrWhiteSpace(nic))
                return false;

            string normalizedNic = Normalize(nic);

            return Regex.IsMatch(normalizedNic, @"^(\d{9}[VX]|\d{12})$");
        }
    }
}