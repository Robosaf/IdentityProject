using IdentityProject.Models;
using Microsoft.EntityFrameworkCore;

namespace IdentityProject.Interfaces
{
    public interface IUserRepository
    {
        public List<AppUser> GetAllAppUsers();
        public AppUser GetAppUserById(string userId);
        public bool DeleteAppUser(AppUser user);
        public bool Save();
    }
}
