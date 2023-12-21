using System.Security.Claims;

namespace IdentityProject.Models
{
    public static class ClaimStore
    {
        public static List<Claim> claimList = new List<Claim>()
        {
            new Claim("Create", "Create"),
            new Claim("Delete", "Delete")
        };
    }
}
