using ACI.Data;
using ACI.DTO.ReqDTO;
using ACI.Entities;
using ACI.IServices.Main.Auth;
using Microsoft.EntityFrameworkCore;

namespace ACI.Service
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _db;

        public AuthService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<IEnumerable<User>> UserDetails(UserDetailsReqDTO req)
        {
            var user = await _db.Users
                .FirstOrDefaultAsync(u => u.Email == req.Email);

            if (user != null && BCrypt.Net.BCrypt.Verify(req.Password, user.Password))
            {
                return new List<User> { user };
            }

            return Enumerable.Empty<User>();
        }
    }
}
