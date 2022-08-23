using Common.ViewModels.ErrorMessages;
using System.ComponentModel.DataAnnotations;

namespace AccountsService.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = ViewModelsErrorMessages.RequiredNotProvided)]
        [StringLength(maximumLength: 15, MinimumLength = 3, ErrorMessage = ViewModelsErrorMessages.OutOfRange)]
        public string UserName { get; set; } = string.Empty;

        [Required(ErrorMessage = ViewModelsErrorMessages.RequiredNotProvided)]
        public string Password { get; set; } = string.Empty;
        
        [Required(ErrorMessage = ViewModelsErrorMessages.RequiredNotProvided)]
        [EmailAddress(ErrorMessage = ViewModelsErrorMessages.IncorrectEmail)]
        public string Email { get; set; } = string.Empty;
    }
}
