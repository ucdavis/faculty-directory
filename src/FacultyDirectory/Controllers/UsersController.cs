using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FacultyDirectory.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class UsersController : ControllerBase
    {
        [HttpGet]
        public ActionResult Get()
        {
            // Return JWT with user info
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes("secretstringhere-verycoolsecret-thebest");
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.Name, "postit"),
                    new Claim(ClaimTypes.GivenName, "Scott"),
                    new Claim(ClaimTypes.Surname, "Kirkland"),
                }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(tokenString);
        }

        // should be POST
        [HttpGet("validate")]
        public ActionResult Validate() {
            // TODO: get auth header
            var jwt = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJ1bmlxdWVfbmFtZSI6InBvc3RpdCIsImdpdmVuX25hbWUiOiJTY290dCIsImZhbWlseV9uYW1lIjoiS2lya2xhbmQiLCJuYmYiOjE1ODEzNTY0NTQsImV4cCI6MTU4MTk2MTI1NCwiaWF0IjoxNTgxMzU2NDU0fQ.gPWdBqRqfsbxj9XxA6RJABQqAzS-4kzSr76Jx1rW93Q";

            var key = Encoding.ASCII.GetBytes("secretstringhere-verycoolsecret-thebest");
            var sharedKey = new SymmetricSecurityKey(key);

            var validationParameters = new TokenValidationParameters
            {
                // Clock skew compensates for server time drift.
                // We recommend 5 minutes or less:
                ClockSkew = TimeSpan.FromMinutes(5),
                // Specify the key used to sign the token:
                IssuerSigningKeys = new [] { sharedKey },
                RequireSignedTokens = true,
                // Ensure the token hasn't expired:
                RequireExpirationTime = true,
                ValidateLifetime = true,
                ValidateAudience = false,
                ValidateIssuer = false
            };

            try
            {
                var claimsPrincipal = new JwtSecurityTokenHandler()
                    .ValidateToken(jwt, validationParameters, out var rawValidatedToken);

                return Ok((JwtSecurityToken)rawValidatedToken);
            }
            catch (SecurityTokenValidationException stvex)
            {
                // The token failed validation!
                // TODO: Log it or display an error.
                throw new Exception($"Token failed validation: {stvex.Message}");
            }
            catch (ArgumentException argex)
            {
                // The token was not well-formed or was invalid for some other reason.
                // TODO: Log it or display an error.
                throw new Exception($"Token was invalid: {argex.Message}");
            }

        }
    }
}