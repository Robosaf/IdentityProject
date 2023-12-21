using Microsoft.AspNetCore.Identity;

namespace IdentityProject.Interfaces
{
    public interface IUserRoleRepository
    {
        public List<IdentityUserRole<string>> GetUserRolesByRoleId(string roleId);
        public IdentityUserRole<string> GetUserRoleByUserId(string userId);

        public List<IdentityUserRole<string>> GetAllUserRoles();
    }
}
