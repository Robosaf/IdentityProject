using IdentityProject.Data;
using IdentityProject.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace IdentityProject.Repositories
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public List<IdentityUserRole<string>> GetAllUserRoles()
        {
            return _context.UserRoles.ToList();
        }

        public List<IdentityUserRole<string>> GetUserRolesByRoleId(string roleId)
        {
            return _context.UserRoles.Where(us => us.RoleId == roleId).ToList();
        }

        public IdentityUserRole<string> GetUserRoleByUserId(string userId)
        {
            return _context.UserRoles.FirstOrDefault(us => us.UserId == userId);
        }
    }
}
