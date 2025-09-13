using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BBltZen
{
    public class JwtServizio
    {
        public const string CHIAVE = "qwffr56rqrg6wggo8g584y4g5y8ew8y9f";
        private IHttpContextAccessor _contextAccessor;
        public JwtServizio(IHttpContextAccessor http)
        {
            _contextAccessor = http;
        }
        public string CreateToken(string username, int utenteId)
        {
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(CHIAVE));
            var token = new JwtSecurityToken(
            expires: DateTime.Now.AddHours(8),
            claims: new List<Claim>
            {
                 new Claim("USERNAME", username),
                 new Claim("UTENTE_ID", utenteId.ToString())
            },
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );
            return new JwtSecurityTokenHandler().WriteToken(token);//mette il token
        }
        public (string username, int utenteId) GetInfoToken()
        {
            if (_contextAccessor.HttpContext == null)
                return (null, 0);
            var userClaims = _contextAccessor.HttpContext.User.Claims;
            if (userClaims.Any())
            {

                var username = userClaims.FirstOrDefault(p => p.Type == "USERNAME").Value;
                var utenteId = int.Parse(userClaims.FirstOrDefault(p => p.Type == "UTENTE_ID").Value);
                return (username, utenteId);
            }
            return (null, 0);//scrive il token
        }
    }
}
