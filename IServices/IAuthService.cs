using ACI.DTO.ReqDTO;
using ACI.Entities;

namespace ACI.IServices.Main.Auth
{
    public interface IAuthService
    {
        Task<IEnumerable<User>> UserDetails(UserDetailsReqDTO req);
    }
}
