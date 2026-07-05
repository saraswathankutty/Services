using ACI.DTO.ResDTO;

namespace Token.IServices
{
    public interface ITokenService
    {
        Task<UserLoginDetailsResDTO> GetUserDetailsFromToken(string token);
    }
}
