using System.Diagnostics;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace ChessApi.Services.Login
{
    public class JwtService
    {
        private readonly string _key;
        private readonly string _issuer;

        public JwtService(IConfiguration config)
        {
            _key = config["JwtSettings:SecretKey"];
            _issuer = config["JwtSettings:Issuer"];
        }

        public string GenerateToken(int userId, string username)
        {
            var claims = new[]
            {
                 new Claim("id", userId.ToString()),
                 new Claim("username", username)
            };
            Debug.WriteLine("_issuer: " + _issuer);
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_key));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _issuer,
                audience: null,
                claims: claims,
                expires: DateTime.UtcNow.AddHours(6),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
