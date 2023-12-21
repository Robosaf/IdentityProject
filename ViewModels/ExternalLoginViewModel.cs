using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace IdentityProject.ViewModels
{
    public class ExternalLoginViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
        public string Name { get; set; }
    }
}
