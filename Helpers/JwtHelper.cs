using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using GridLink.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace GridLink.Helpers
{
    public class JwtHelper
    {

        private readonly JwtSettings _jwtsettings;

        public JwtHelper(IOptions<JwtSettings> JwtOptions)
        {
            _jwtsettings = JwtOptions.Value;
        }

        public (string token, DateTime Expiration) GenerateToken(Users users)
        {
            var tokenhandler = new JwtSecurityTokenHandler();

            byte[] key = Encoding.UTF8.GetBytes(_jwtsettings.Secret);

            DateTime expiration = DateTime.UtcNow.AddMinutes(_jwtsettings.DurationInMinutes);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, users.Id),
                new Claim(ClaimTypes.Name, users.FullName),
                new Claim(ClaimTypes.Email, users.Email),
                new Claim(ClaimTypes.Role, users.Role),
                new Claim("nic", users.NIC),
                new Claim("status", users.Status)
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = _jwtsettings.Issuer,
                Audience = _jwtsettings.Audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                    )
            };

            SecurityToken token = tokenhandler.CreateToken(tokenDescriptor);
            string tokenString = tokenhandler.WriteToken(token);

            return (tokenString, expiration);
        }

    }
}
