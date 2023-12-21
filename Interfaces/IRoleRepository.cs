using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityProject.Interfaces
{
    public interface IRoleRepository
    {
        public List<IdentityRole> GetAllRoles();
        public IdentityRole GetRoleById(string roleId);
    }
}
