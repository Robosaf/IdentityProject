using System.ComponentModel.DataAnnotations;

namespace IdentityProject.ViewModels
{
    public class ForgotPasswordViewModel
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}
