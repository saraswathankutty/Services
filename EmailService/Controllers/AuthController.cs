using ACI.DTO.ReqDTO;
using ACI.IServices.Main.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ACI.Auth;

namespace ACI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IAuthService _authService;

        public AuthController(IOptions<JwtSettings> options, IAuthService authService)
        {
            _jwtSettings = options.Value;
            _authService = authService;
        }

        [HttpPost]
        public async Task<IActionResult> Authentication([FromBody] UserDetailsReqDTO req)
        {
            var res = await _authService.UserDetails(req);
            if (res == null || !res.Any())
            {
                return Unauthorized();
            }

            var user = res.First();
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenkey = Encoding.UTF8.GetBytes(_jwtSettings.SecretKey);
            var tokendescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Name),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim("Id", Convert.ToString(user.Id))
                    }
                ),
                Expires = DateTime.Now.AddMinutes(60),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenkey), SecurityAlgorithms.HmacSha256)
            };
            var token = tokenHandler.CreateToken(tokendescriptor);
            string FinalToken = tokenHandler.WriteToken(token);
            return Ok(new { Token = FinalToken });
        }
    }
}
