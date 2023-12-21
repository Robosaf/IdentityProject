using IdentityProject.Data;
using IdentityProject.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace IdentityProject.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public List<IdentityRole> GetAllRoles()
        {
            return _context.Roles.ToList();
        }

        public IdentityRole GetRoleById(string roleId)
        {
            return _context.Roles.FirstOrDefault(r => r.Id == roleId);
        }
    }
}
