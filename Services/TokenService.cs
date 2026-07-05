using ACI.DTO.ResDTO;
using ACI.Auth;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Reflection;
using System.Text;
using Token.IServices;
using static ACI.Helper.GeneralHelperService;

namespace Token.Services
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly JwtSettings _jwt;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration configuration, ILogger<TokenService> logger, IOptions<JwtSettings> jwt)
        {
            _config = configuration;
            _logger = logger;
            _jwt = jwt.Value;
        }

        public async Task<UserLoginDetailsResDTO> GetUserDetailsFromToken(string token)
        {
            UserLoginDetailsResDTO us = new ();
            token = token?.Split(" ").Last();
            JwtSecurityToken jwtSecurityToken = null;
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwt.SecretKey)),
                    ValidateIssuer = false,
                    ValidateAudience = false
                };

                var principal = tokenHandler.ValidateToken(token, validationParameters, out SecurityToken validatedToken);
                jwtSecurityToken = validatedToken as JwtSecurityToken;

                if (jwtSecurityToken != null)
                {
                    foreach (var claim in jwtSecurityToken.Claims)
                    {
                        string value = claim.Value.Replace("{}", "");
                        foreach(var item in GetPropertyNameList("ACI.DTO.ResDTO.UserLoginDetailsResDTO"))
                        {
                            if (claim.Type == item)
                            {
                                PropertyInfo propertyInfo = us.GetType().GetProperty(item);
                                if (propertyInfo != null)
                                {
                                    object convertedValue = ConvertToType(value, propertyInfo.PropertyType);
                                    if (convertedValue != null)
                                    {
                                        propertyInfo.SetValue(us, convertedValue);
                                    }
                                }
                            }
                        }
                    }
                }
                return us;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "TokenService :: GetUserDetailsFromToken :: Error");
            }
            return await Task.FromResult(new UserLoginDetailsResDTO());
        }
    }
}
