using IdentityProject.Data;
using IdentityProject.Interfaces;
using IdentityProject.Models;

namespace IdentityProject.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRepository(ApplicationDbContext applicationDbContext)
        {
            _context = applicationDbContext;
        }

        public List<AppUser> GetAllAppUsers()
        {
            return _context.AppUser.ToList();
        }

        public AppUser GetAppUserById(string userId)
        {
            return _context.AppUser.FirstOrDefault(user => user.Id == userId);
        }

        public bool DeleteAppUser(AppUser user)
        {
            _context.Remove(user);
            return Save();
        }
        public bool Save()
        {
            var saved = _context.SaveChanges();
            return saved > 0 ? true : false;
        }
    }
}
